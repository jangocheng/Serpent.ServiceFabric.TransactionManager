namespace TransactionManager.Interfaces
{
    using Microsoft.ServiceFabric.Actors;

    public interface ITransactionManagerEvents : IActorEvents
    {
        void RollbackTransaction();

        void CommitTransaction();
    }
}