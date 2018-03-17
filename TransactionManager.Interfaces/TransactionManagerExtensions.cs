namespace TransactionManager.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    public static class TransactionManagerExtensions
    {
        public static Task StartTransactionAsync(this ITransactionManager transactionManager, TransactionProperties transactionProperties)
        {
            return transactionManager.BeginTransactionAsync(transactionProperties, CancellationToken.None);
        }

        public static Task StartTransactionAsync(this ITransactionManager transactionManager)
        {
            return transactionManager.BeginTransactionAsync(default(TransactionProperties), CancellationToken.None);
        }

    }
}