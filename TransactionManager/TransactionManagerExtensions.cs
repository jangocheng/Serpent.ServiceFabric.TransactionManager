namespace TransactionManager
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using global::TransactionManager.Interfaces;
    using global::TransactionManager.Models;

    internal static class TransactionManagerExtensions
    {
        public static async Task<TransactionStatus> GetTransactionStateStatusAsync(
            this TransactionManager transactionManager,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await transactionManager.StateManager.GetStateAsync<TransactionStatus>(StateConstants.TransactionStatus, cancellationToken);
        }

        public static async Task SetTransactionStateStatusAsync(
            this TransactionManager transactionManager,
            TransactionStatus transactionStatus,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await transactionManager.StateManager.SetStateAsync(StateConstants.TransactionStatus, transactionStatus, cancellationToken);
        }


        public static async Task<TransactionParticipants> GetTransactionParticipantsAsync(
            this TransactionManager transactionManager,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await transactionManager.StateManager.GetStateAsync<TransactionParticipants>(StateConstants.TransactionParticipants, cancellationToken);
        }

        public static async Task SetTransactionParticipantsAsync(
            this TransactionManager transactionManager,
            TransactionParticipants transactionParticipants,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await transactionManager.StateManager.SetStateAsync(StateConstants.TransactionParticipants, transactionParticipants, cancellationToken);
        }

        public static async Task AddTransactionParticipantAsync(
            this TransactionManager transactionManager,
            TransactionParticipantId transactionParticipantId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var participants = await transactionManager.GetTransactionParticipantsAsync(cancellationToken);

            if (!participants.Contains(transactionParticipantId))
            {
                participants = participants.Add(new TransactionParticipant(transactionParticipantId, TransactionParticipantStatus.Added));
            }

            await transactionManager.SetTransactionParticipantsAsync(participants, cancellationToken);
        }

        internal static Interfaces.TransactionStatus ConvertToPublicStatus(
            this TransactionManager transactionManager,
            TransactionStatus transactionStatus)
        {
            switch (transactionStatus)
            {
                case TransactionStatus.Unknown:
                    return Interfaces.TransactionStatus.Unknown;
                case TransactionStatus.Created:
                    return Interfaces.TransactionStatus.Created;
                case TransactionStatus.Active:
                    return Interfaces.TransactionStatus.Active;
                case TransactionStatus.Committing:
                    return Interfaces.TransactionStatus.Committing;
                case TransactionStatus.Committed:
                    return Interfaces.TransactionStatus.Committed;
                case TransactionStatus.RollbackStarted:
                    return Interfaces.TransactionStatus.RollbackStarted;
                case TransactionStatus.RollbackDone:
                    return Interfaces.TransactionStatus.RollbackDone;
                case TransactionStatus.RollbackFailed:
                    return Interfaces.TransactionStatus.RollbackFailed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(transactionStatus), transactionStatus, null);
            }
        }
    }
}
