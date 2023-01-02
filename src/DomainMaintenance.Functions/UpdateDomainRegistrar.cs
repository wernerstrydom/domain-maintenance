using System;
using System.Threading.Tasks;
using Amazon.Route53Domains;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DomainMaintenance.Functions;

public class UpdateDomainRegistrar
{
    [FunctionName("UpdateDomainRegistrar")]
    public async Task Run([QueueTrigger(Constant.RegistrarUpdateQueue)] string domain,
        [Queue(Constant.NotificationQueueName)]
        ICollector<string> notificationQueue,
        [Table(Constant.ContactsTableName, "Contact", "default")]
        Contact defaultContact,
        [Table(Constant.ContactsTableName, "Contact", "{queueTrigger}")]
        Contact domainContact,
        ILogger log)
    {
        if (log == null) throw new ArgumentNullException(nameof(log));

        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(domain));

        log.LogInformation("Checking the contact details for domain {DomainName}", domain);


        if (domainContact == null)
        {
            log.LogInformation("No contact details found for domain '{DomainName}'. Using default contact details.",
                domain);
            domainContact = defaultContact;
        }

        if (defaultContact == null)
        {
            log.LogError("No default contact details found. Please add a default contact to the table storage.");
            return;
        }


        var expected = new DomainDetail
        {
            AdminContact = domainContact,
            RegistrantContact = domainContact,
            TechContact = domainContact
        };

        var client = new AmazonRoute53DomainsClient();
        var actual = await client.GetDomainDetailAsync(domain);
        if (actual == expected)
        {
            log.LogInformation("The contact details of domain '{DomainName}' is already up to date", domain);
            return;
        }

        log.LogInformation("Updating domain '{DomainName}' with new contact details", domain);
        await client.SetDomainDetail(expected, domain);

        notificationQueue.Add($"Updating domain details for '{domain}' succeeded");
    }
}