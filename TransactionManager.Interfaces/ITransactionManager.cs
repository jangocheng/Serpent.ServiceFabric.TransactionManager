using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]

namespace TransactionManager.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Actors;

    /// <summary>
    ///     This interface defines the methods exposed by an actor.
    ///     Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface ITransactionManager : IActor, IActorEventPublisher<ITransactionManagerEvents>
    {
        Task<TransactionId> BeginTransactionAsync(TransactionProperties transactionProperties, CancellationToken cancellationToken);

        Task<TransactionParticipantId> RegisterTransactionParticipantAsync(CancellationToken cancellationToken);

        Task<TransactionStatus> GetTransactionStatusAsync(CancellationToken cancellationToken);

        // Not sure if we need this method
        Task UnregisterTransactionParticipantAsync(TransactionParticipantId transactionParticipantId, CancellationToken cancellationToken);

        Task ReportTransactionParticipantCommittedAsync(TransactionParticipantId transactionParticipantId, CancellationToken cancellationToken);

        Task ReportTransactionParticipantRolledBackAsync(TransactionParticipantId transactionParticipantId, CancellationToken cancellationToken);

        Task RollbackTransactionAsync(CancellationToken cancellationToken);

        Task CommitTransactionAsync(CancellationToken cancellationToken);
    }
}