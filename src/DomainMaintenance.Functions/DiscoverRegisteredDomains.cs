using System;
using Azure.Data.Tables;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Amazon.Route53;
using Amazon.Route53.Model;
using Amazon.Route53Domains;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using Amazon.Route53Domains.Model;

namespace DomainMaintenance.Functions
{
    public class DiscoverRegisteredDomains
    {
        [FunctionName("DiscoverRegisteredDomains")]

        public async Task Run(
            [TimerTrigger("0 0 10 * * *")] TimerInfo timer,
            [Table("domains")] TableClient table,
            CancellationToken cancellationToken,
            ILogger log)
        {
            var registered = await DiscoverDomains(cancellationToken);

            // we're using the table as a cache to know which domains we processed before
            var cached = await table.QueryAsync<RegisteredDomain>(maxPerPage: 1000).ToDictionaryAsync(d => d.DomainName).ConfigureAwait(false);

            List<string> added = new(), deleted = new(), updated = new();
            foreach (var (domainName, domain) in registered)
            {
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
                }
                else
                {
                    // needs to be added
                    log.LogInformation($"{domainName} does not exists in cache");
                    added.Add(domainName);
                }
            }

            foreach (var (domainName, domain) in cached)
            {
                if (!registered.ContainsKey(domainName))
                {
                    log.LogInformation($"{domainName} exists in cache but not in registrar and should be deleted");
                    deleted.Add(domainName);
                }
            }

            foreach (var domainName in added)
            {
                var domain = registered[domainName];
                await table.AddEntityAsync(domain, cancellationToken);
            }

            foreach (var domainName in deleted)
            {
                var domain = registered[domainName];
                await table.DeleteEntityAsync(domain.PartitionKey, domain.RowKey, default, cancellationToken);
            }

            foreach (var domainName in updated)
            {
                var other = cached[domainName];
                var domain = registered[domainName];
                await table.UpdateEntityAsync(domain, Azure.ETag.All);
            }
        }


        private static async Task<Dictionary<string, RegisteredDomain>> DiscoverDomains(CancellationToken cancellationToken)
        {
            var domains = new Dictionary<string, RegisteredDomain>();
            var client = new AmazonRoute53DomainsClient();

            ListDomainsResponse response;
            ListDomainsRequest request = new ListDomainsRequest { MaxItems = 10 };

            do
            {
                response = await client.ListDomainsAsync(request, cancellationToken);
                foreach (var domain in response.Domains)
                {
                    RegisteredDomain d = new RegisteredDomain(domain.DomainName, domain.Expiry, domain.AutoRenew, domain.TransferLock);
                    domains.Add(d.DomainName, d);
                }
                request.Marker = response.NextPageMarker;
            } while (!string.IsNullOrEmpty(response.NextPageMarker));

            return domains;
        }
    }
}
