using System;
using Amazon.Route53Domains;
using Amazon.Route53Domains.Model;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection.Emit;

namespace DomainMaintenance.Functions
{
    public class UpdateDomainRegistrar
    {
        [FunctionName("UpdateDomainRegistrar")]
        public async Task Run([QueueTrigger(Constant.RegistrarUpdateQueue)]string domain, 
            [Queue(Constant.NotificationQueueName)] ICollector<string> notificationQueue,
            [Table(Constant.ContactsTableName, "Contact", "default")] Contact defaultContact, 
            [Table(Constant.ContactsTableName, "Contact", "{queueTrigger}")] Contact domainContact, 
            ILogger log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            
            if (string.IsNullOrWhiteSpace(domain))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(domain));
            
            log.LogInformation("Checking the contact details for domain {DomainName}", domain);
            
            
            if (domainContact == null)
            {
                log.LogInformation("No contact details found for domain '{DomainName}'. Using default contact details.", domain);
                domainContact = defaultContact;
            }
            
            if (defaultContact == null)
            {
                log.LogError("No default contact details found. Please add a default contact to the table storage.");
                return;
            }

            
            var expected = new DomainDetail()
            {
                AdminContact = domainContact,
                RegistrantContact = domainContact,
                TechContact = domainContact,
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
            
            notificationQueue.Add($"Domain '{domain}' has been updated with new contact details");
        }
    }

    public static class AmazonRoute53DomainsClientExtensions
    {
        public static async Task<DomainDetail> GetDomainDetailAsync(this AmazonRoute53DomainsClient client, string domainName)
        {
            var response = await client.GetDomainDetailAsync(new GetDomainDetailRequest() { DomainName = domainName });
            var result = response.ToDomainDetail();
            return result;
        }

        public static async Task SetDomainDetail(this AmazonRoute53DomainsClient client, DomainDetail expected, string domainName)
        {
            await client.UpdateDomainContactAsync(expected.ToUpdateDomainContactRequest(domainName));
        }
    }
    
    public static class JsonExtensions
    {
        public static string ToJson<T>(this T obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
        }
    }
}

