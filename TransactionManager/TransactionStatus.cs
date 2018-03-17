namespace TransactionManager
{
    internal enum TransactionStatus
    {
        Unknown,
        Created,
        Active,
        Committing,
        Committed,
        RollingBack,
        RolledBack,
    }
}