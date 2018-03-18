namespace TransactionManager.Interfaces
{
    public enum TransactionStatus
    {
        Unknown = 0,
        Created = 10,
        Active = 20,
        CommitStarted = 1000,
        Committed = 1001,
        RollbackStarted = 2000,
        RollbackDone = 2001,
        RollbackFailed = 2001,
    }
}
