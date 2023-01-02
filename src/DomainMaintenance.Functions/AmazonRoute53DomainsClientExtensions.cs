using System.Threading.Tasks;
using Amazon.Route53Domains;
using Amazon.Route53Domains.Model;

namespace DomainMaintenance.Functions;

public static class AmazonRoute53DomainsClientExtensions
{
    public static async Task<DomainDetail> GetDomainDetailAsync(this AmazonRoute53DomainsClient client,
        string domainName)
    {
        var response = await client.GetDomainDetailAsync(new GetDomainDetailRequest { DomainName = domainName });
        var result = response.ToDomainDetail();
        return result;
    }

    public static async Task SetDomainDetail(this AmazonRoute53DomainsClient client, DomainDetail expected,
        string domainName)
    {
        await client.UpdateDomainContactAsync(expected.ToUpdateDomainContactRequest(domainName));
    }
}