namespace TransactionManager.Models
{
    public enum TransactionParticipantStatus
    {
        Added,

        UncommittedChanges,

        Committed,

        Rolledback
    }
}