namespace DomainMaintenance.Functions;

public record DomainDetail
{
    public Contact AdminContact { get; init; }
    public Contact RegistrantContact { get; init; }
    public Contact TechContact { get; init; }
}