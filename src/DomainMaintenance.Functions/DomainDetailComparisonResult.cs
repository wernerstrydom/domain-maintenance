using System.Collections.Generic;

namespace DomainMaintenance.Functions;

public record DomainDetailComparisonResult(string FieldName, List<ContactComparisonResult> Differences);