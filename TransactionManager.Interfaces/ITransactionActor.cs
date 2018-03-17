namespace TransactionManager.Interfaces
{
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Actors;

    public interface ITransactionActor : IActor
    {
        Task RollbackTransactionAsync(TransactionId transactionId);

        Task CommitTransactionAsync(TransactionId transactionId);
    }
}