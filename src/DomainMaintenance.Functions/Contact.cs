//
// Contact.cs
//
// Author:
//       Werner Strydom <hello@wernerstrydom.com>
//
// Copyright (c) 2023 Copyright (c) 2023 Werner Strydom
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using Amazon.Route53Domains;
using Amazon.Route53Domains.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing.Internal;

namespace DomainMaintenance.Functions
{
    public record Contact
    {
        public string AddressLine1 { get; set; }
        
        public string AddressLine2 { get; set; }
        
        public string City { get; set; }

        public string ContactType { get; set; }

        public string CountryCode { get; set; }

        public string Email { get; set; }

        public string Fax { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string OrganizationName { get; set; }

        public string PhoneNumber { get; set; }

        public string State { get; set; }
        
        public string ZipCode { get; set; }

    }

    public static class ContactExtensions
    {
        public static Contact ToContact(this ContactDetail that)
        {
            return new Contact()
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
            return new ContactDetail()
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
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.AddressLine1), left.AddressLine1, right.AddressLine1));
            }
            
            if (left.AddressLine2 != right.AddressLine2)
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.AddressLine2), left.AddressLine2, right.AddressLine2));
            }
            
            if (left.City != right.City)
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.City), left.City, right.City));
            }
            
            if (left.ContactType != right.ContactType)
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.ContactType), left.ContactType, right.ContactType));
            }
            
            if (left.CountryCode != right.CountryCode)
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.CountryCode), left.CountryCode, right.CountryCode));
            }
            
            if (left.Email != right.Email)
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.Email), left.Email, right.Email));
            }
            
            if (left.Fax != right.Fax)
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.Fax), left.Fax, right.Fax));
            }
            
            if (left.FirstName != right.FirstName)
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.FirstName), left.FirstName, right.FirstName));
            }
            
            if (left.LastName != right.LastName)
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.LastName), left.LastName, right.LastName));
            }
            
            if (left.OrganizationName != right.OrganizationName)
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.OrganizationName), left.OrganizationName, right.OrganizationName));
            }
            
            if (left.PhoneNumber != right.PhoneNumber)
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.PhoneNumber), left.PhoneNumber, right.PhoneNumber));
            }
            
            if (left.State != right.State)
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.State), left.State, right.State));
            }
            
            if (left.ZipCode != right.ZipCode)
            {
                differences.Add(new ContactComparisonResult(nameof(Contact.ZipCode), left.ZipCode, right.ZipCode));
            }
            
            return differences;
        }
    }

    public record ContactComparisonResult(string FieldName, string Left, string Right);
    
    public record DomainDetailComparisonResult(string FieldName, List<ContactComparisonResult> Differences);
}

