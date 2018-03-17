namespace TransactionManager
{
    using System.Threading;
    using System.Threading.Tasks;

    using global::TransactionManager.Interfaces;
    using global::TransactionManager.Models;

    internal static class TransactionManagerExtensions
    {
        public static async Task<TransactionStatus> GetTransactionStatusAsync(this TransactionManager transactionManager, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await transactionManager.StateManager.GetStateAsync<TransactionStatus>(StateConstants.TransactionStatus, cancellationToken);
        }

        public static async Task SetTransactionStatusAsync(this TransactionManager transactionManager, TransactionStatus transactionStatus, CancellationToken cancellationToken = default(CancellationToken))
        {
            await transactionManager.StateManager.SetStateAsync(StateConstants.TransactionStatus, transactionStatus, cancellationToken);
        }


        public static async Task<TransactionParticipants> GetTransactionParticipantsAsync(this TransactionManager transactionManager, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await transactionManager.StateManager.GetStateAsync<TransactionParticipants>(StateConstants.TransactionParticipants, cancellationToken);
        }

        public static async Task SetTransactionParticipantsAsync(this TransactionManager transactionManager, TransactionParticipants transactionParticipants, CancellationToken cancellationToken = default(CancellationToken))
        {
            await transactionManager.StateManager.SetStateAsync(StateConstants.TransactionParticipants, transactionParticipants, cancellationToken);
        }

        public static async Task AddOrUpdateTransactionParticipantAsync(this TransactionManager transactionManager, TransactionParticipantId transactionParticipantId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var participants = await transactionManager
                                   .GetTransactionParticipantsAsync(cancellationToken);

            if (!participants.Contains(transactionParticipantId))
            {
                participants = participants.Add(transactionParticipantId);
            }

            await transactionManager.SetTransactionParticipantsAsync(participants, cancellationToken);
        }
    }
}