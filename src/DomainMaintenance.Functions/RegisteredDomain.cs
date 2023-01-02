//
// RegisteredDomain.cs
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
using System.Collections.Generic;
using Azure;
using Azure.Data.Tables;

namespace DomainMaintenance.Functions;

public class RegisteredDomain : ITableEntity, IEquatable<RegisteredDomain>
{
    public RegisteredDomain(string domainName, DateTime expiry, bool autoRenew, bool transferLock)
    {
        PartitionKey = "RegisteredDomain";
        DomainName = domainName;
        RowKey = domainName;
        Expiry = expiry;
        AutoRenew = autoRenew;
        TransferLock = transferLock;
    }

    public RegisteredDomain()
    {
    }

    public string DomainName { get; set; }
    public DateTime Expiry { get; set; }
    public bool AutoRenew { get; set; }
    public bool TransferLock { get; set; }

    public bool Equals(RegisteredDomain other)
    {
        return other is not null &&
               DomainName == other.DomainName &&
               Expiry == other.Expiry &&
               AutoRenew == other.AutoRenew &&
               TransferLock == other.TransferLock;
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as RegisteredDomain);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DomainName, Expiry, AutoRenew, TransferLock);
    }

    public static bool operator ==(RegisteredDomain left, RegisteredDomain right)
    {
        return EqualityComparer<RegisteredDomain>.Default.Equals(left, right);
    }

    public static bool operator !=(RegisteredDomain left, RegisteredDomain right)
    {
        return !(left == right);
    }
}