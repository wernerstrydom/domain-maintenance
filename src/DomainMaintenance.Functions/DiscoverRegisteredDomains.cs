using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Route53Domains;
using Amazon.Route53Domains.Model;
using Azure;
using Azure.Data.Tables;
using Humanizer;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DomainMaintenance.Functions;

public class DiscoverRegisteredDomains
{
    [FunctionName("DiscoverRegisteredDomains")]
    public async Task Run(
        [TimerTrigger(Constant.DiscoverRegisteredDomainsCronExpression)]
        TimerInfo timer,
        [Queue(Constant.NotificationQueueName)]
        ICollector<string> notificationQueue,
        [Table(Constant.DomainsTableName)] TableClient table,
        [Queue(Constant.RegistrarUpdateQueue)] ICollector<string> registrarUpdateQueue,
        CancellationToken cancellationToken,
        ILogger log)
    {
        var registered = await DiscoverDomains(log, cancellationToken);

        // we're using the table as a cache to know which domains we processed before
        var cached = await table.QueryAsync<RegisteredDomain>(maxPerPage: 1000).ToDictionaryAsync(d => d.DomainName)
            .ConfigureAwait(false);

        List<string> added = new(), deleted = new(), updated = new();
        foreach (var (domainName, domain) in registered)
            if (domain.Expiry.AddDays(31) < DateTime.UtcNow)
            {
                log.LogInformation($"{domainName} has expired and should be deleted");
                deleted.Add(domainName);
            }
            else if (cached.ContainsKey(domainName))
            {
                log.LogInformation($"{domainName} exists in cache");
                if (cached[domainName] != registered[domainName])
                {
                    // exists in cache, and should be updated
                    log.LogInformation($"{domainName} needs to be updated in cache");
                    updated.Add(domainName);
                }

                registrarUpdateQueue.Add(domain.DomainName);
            }
            else
            {
                // needs to be added
                log.LogInformation($"{domainName} does not exists in cache");
                added.Add(domainName);
                registrarUpdateQueue.Add(domain.DomainName);
            }

        foreach (var (domainName, domain) in cached)
            if (!registered.ContainsKey(domainName))
            {
                log.LogInformation($"{domainName} exists in cache but not in registrar and should be deleted");
                deleted.Add(domainName);
            }

        foreach (var domainName in added)
        {
            var domain = registered[domainName];
            await table.AddEntityAsync(domain, cancellationToken);
            log.LogInformation("Added '{DomainName}' to the cache", domainName);
        }

        foreach (var domainName in deleted)
        {
            var domain = registered[domainName];
            await table.DeleteEntityAsync(domain.PartitionKey, domain.RowKey, default, cancellationToken);
            log.LogInformation("Deleted '{DomainName}' from the cache", domainName);
        }

        foreach (var domainName in updated)
        {
            var other = cached[domainName];
            var domain = registered[domainName];

            await table.UpdateEntityAsync(domain, ETag.All, cancellationToken: cancellationToken);
            log.LogInformation("Updated '{DomainName}' in the cache", domainName);
        }

        await Notify(notificationQueue, registered, added, deleted, updated);
    }

    private static async Task Notify(ICollector<string> notificationQueue, ICollection registered,
        IEnumerable<string> added, IEnumerable<string> deleted, IEnumerable<string> updated)
    {
        StringWriter writer = new();
        await writer.WriteAsync($"Processing of '{registered.Count}' domains succeeded. ");
        if (added.Any())
            await writer.WriteAsync($"Scheduled {added.Humanize()} to be added. ");
        else
            await writer.WriteAsync("No domains to be added. ");


        if (deleted.Any())
            await writer.WriteAsync($"Scheduled {deleted.Humanize()} to deleted. ");
        else
            await writer.WriteAsync("No domains to be deleted. ");

        if (updated.Any())
            await writer.WriteAsync($"Successfully updated {updated.Humanize()}. ");
        else
            await writer.WriteAsync("No domains to be updated. ");

        notificationQueue.Add(writer.ToString());
    }

    private static void LogEnvironment(ILogger log)
    {
        string[] environmentVariableNames =
            { "AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY", "AWS_REGION", "AWS_DEFAULT_REGION" };
        foreach (var environmentVariableName in environmentVariableNames)
        {
            var value = Environment.GetEnvironmentVariable(environmentVariableName);
            if (string.IsNullOrWhiteSpace(value))
                log.LogInformation("{EnvironmentVariable}: missing", environmentVariableName);
            else
                log.LogInformation("{EnvironmentVariable}: exists", environmentVariableName);
        }
    }

    private static async Task<Dictionary<string, RegisteredDomain>> DiscoverDomains(ILogger log,
        CancellationToken cancellationToken)
    {
        LogEnvironment(log);
        try
        {
            var client = new AmazonRoute53DomainsClient();
            var domains = new Dictionary<string, RegisteredDomain>();
            ListDomainsResponse response;
            var request = new ListDomainsRequest { MaxItems = 100 };
            do
            {
                log.LogInformation("Getting the list of domains registered in AWS Route53");
                response = await client.ListDomainsAsync(request, cancellationToken);
                foreach (var domain in response.Domains)
                {
                    var d = new RegisteredDomain(domain.DomainName, domain.Expiry, domain.AutoRenew,
                        domain.TransferLock);
                    domains.Add(d.DomainName, d);
                }

                request.Marker = response.NextPageMarker;
            } while (!string.IsNullOrEmpty(response.NextPageMarker));

            return domains;
        }
        catch (AmazonRoute53DomainsException e)
        {
            log.LogError(e, "Error trying to get the list of domains: {Message} ({ErrorCode})", e.Message, e.ErrorCode);
            throw;
        }
    }
}