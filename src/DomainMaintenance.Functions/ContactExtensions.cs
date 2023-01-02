using System.Collections.Generic;
using Amazon.Route53Domains.Model;

namespace DomainMaintenance.Functions;

public static class ContactExtensions
{
    public static Contact ToContact(this ContactDetail that)
    {
        return new Contact
        {
            AddressLine1 = that.AddressLine1,
            AddressLine2 = that.AddressLine2,
            City = that.City,
            ContactType = that.ContactType,
            CountryCode = that.CountryCode,
            Email = that.Email,
            Fax = that.Fax,
            FirstName = that.FirstName,
            LastName = that.LastName,
            OrganizationName = that.OrganizationName,
            PhoneNumber = that.PhoneNumber,
            State = that.State,
            ZipCode = that.ZipCode
        };
    }

    public static ContactDetail ToContactDetail(this Contact that)
    {
        return new ContactDetail
        {
            AddressLine1 = that.AddressLine1,
            AddressLine2 = that.AddressLine2,
            City = that.City,
            ContactType = that.ContactType,
            CountryCode = that.CountryCode,
            Email = that.Email,
            Fax = that.Fax,
            FirstName = that.FirstName,
            LastName = that.LastName,
            OrganizationName = that.OrganizationName,
            PhoneNumber = that.PhoneNumber,
            State = that.State,
            ZipCode = that.ZipCode
        };
    }

    public static List<ContactComparisonResult> GetDifferences(this Contact left, Contact right)
    {
        var differences = new List<ContactComparisonResult>();

        if (left.AddressLine1 != right.AddressLine1)
            differences.Add(new ContactComparisonResult(nameof(Contact.AddressLine1), left.AddressLine1,
                right.AddressLine1));

        if (left.AddressLine2 != right.AddressLine2)
            differences.Add(new ContactComparisonResult(nameof(Contact.AddressLine2), left.AddressLine2,
                right.AddressLine2));

        if (left.City != right.City)
            differences.Add(new ContactComparisonResult(nameof(Contact.City), left.City, right.City));

        if (left.ContactType != right.ContactType)
            differences.Add(new ContactComparisonResult(nameof(Contact.ContactType), left.ContactType,
                right.ContactType));

        if (left.CountryCode != right.CountryCode)
            differences.Add(new ContactComparisonResult(nameof(Contact.CountryCode), left.CountryCode,
                right.CountryCode));

        if (left.Email != right.Email)
            differences.Add(new ContactComparisonResult(nameof(Contact.Email), left.Email, right.Email));

        if (left.Fax != right.Fax)
            differences.Add(new ContactComparisonResult(nameof(Contact.Fax), left.Fax, right.Fax));

        if (left.FirstName != right.FirstName)
            differences.Add(new ContactComparisonResult(nameof(Contact.FirstName), left.FirstName, right.FirstName));

        if (left.LastName != right.LastName)
            differences.Add(new ContactComparisonResult(nameof(Contact.LastName), left.LastName, right.LastName));

        if (left.OrganizationName != right.OrganizationName)
            differences.Add(new ContactComparisonResult(nameof(Contact.OrganizationName), left.OrganizationName,
                right.OrganizationName));

        if (left.PhoneNumber != right.PhoneNumber)
            differences.Add(new ContactComparisonResult(nameof(Contact.PhoneNumber), left.PhoneNumber,
                right.PhoneNumber));

        if (left.State != right.State)
            differences.Add(new ContactComparisonResult(nameof(Contact.State), left.State, right.State));

        if (left.ZipCode != right.ZipCode)
            differences.Add(new ContactComparisonResult(nameof(Contact.ZipCode), left.ZipCode, right.ZipCode));

        return differences;
    }
}