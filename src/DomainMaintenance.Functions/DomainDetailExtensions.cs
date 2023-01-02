using System.Collections.Generic;
using Amazon.Route53Domains.Model;

namespace DomainMaintenance.Functions;

public static class DomainDetailExtensions
{
    public static DomainDetail ToDomainDetail(this GetDomainDetailResponse that)
    {
        return new DomainDetail
        {
            AdminContact = that.AdminContact.ToContact(),
            RegistrantContact = that.RegistrantContact.ToContact(),
            TechContact = that.TechContact.ToContact()
        };
    }

    public static UpdateDomainContactRequest ToUpdateDomainContactRequest(this DomainDetail that, string domainName)
    {
        return new UpdateDomainContactRequest
        {
            AdminContact = that.AdminContact.ToContactDetail(),
            RegistrantContact = that.RegistrantContact.ToContactDetail(),
            TechContact = that.TechContact.ToContactDetail(),
            DomainName = domainName
        };
    }

    public static List<DomainDetailComparisonResult> GetDifferences(this DomainDetail left, DomainDetail right)
    {
        var differences = new List<DomainDetailComparisonResult>();

        if (left.AdminContact != right.AdminContact)
        {
            var d = left.AdminContact.GetDifferences(right.AdminContact);
            differences.Add(new DomainDetailComparisonResult(nameof(DomainDetail.AdminContact), d));
        }

        if (left.RegistrantContact != right.RegistrantContact)
        {
            var d = left.RegistrantContact.GetDifferences(right.RegistrantContact);
            differences.Add(new DomainDetailComparisonResult(nameof(DomainDetail.RegistrantContact), d));
        }

        if (left.TechContact != right.TechContact)
        {
            var d = left.TechContact.GetDifferences(right.TechContact);
            differences.Add(new DomainDetailComparisonResult(nameof(DomainDetail.TechContact), d));
        }

        return differences;
    }
}