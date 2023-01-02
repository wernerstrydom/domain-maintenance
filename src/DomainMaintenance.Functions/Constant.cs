namespace DomainMaintenance.Functions;

public static class Constant
{
#if DEBUG
    public const string DiscoverRegisteredDomainsCronExpression = "0/30 * * * * *";
#else
        public const string DiscoverRegisteredDomainsCronExpression = "0 0 19 * * *";
#endif
    public const string ContactsTableName = "contacts";
    public const string DomainsTableName = "domains";
    public const string RegistrarUpdateQueue = "domain-registrar-update";
    public const string NotificationQueueName = "notifications";
}