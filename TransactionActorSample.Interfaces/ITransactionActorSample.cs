using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]

namespace TransactionActorSample.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    using TransactionManager.Interfaces;

    /// <summary>
    ///     This interface defines the methods exposed by an actor.
    ///     Clients use this interface to interact with the actor that implements it.
    /// </summary>ActorService
    public interface ITransactionActorSample : ITransactionActor
    {
        Task<Order> GetOrderAsync(TransactionId transactionId);

        Task UpdateOrderAsync(TransactionId transactionId, Order order);
    }
}