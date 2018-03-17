namespace TransactionActorSample
{
    using System;
    using System.Fabric;
    using System.Threading.Tasks;

    using global::TransactionActorSample.Interfaces;

    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;
    using Microsoft.ServiceFabric.Actors.Runtime;

    using TransactionManager.Interfaces;

    [StatePersistence(StatePersistence.Persisted)]
    [ActorService(Name = "TransactionActorSample")]
    internal class TransactionActorSample : Actor, ITransactionActorSample
    {
        /// <summary>
        ///     Initializes a new instance of TransactionActorSample
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public TransactionActorSample(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }


        /// <summary>
        ///     This method is called whenever an actor is activated.
        ///     An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization
            return this.StateManager.TryAddStateAsync("count", 0);
        }

        public async Task RollbackTransactionAsync(TransactionId transactionId)
        {
            var proxy = ActorProxy.Create<ITransactionManager>(transactionId.ToActorId(), ApplicationName);

            var handler = new TransactionManagerEventsHandler();

            await proxy.SubscribeAsync<ITransactionManagerEvents>(handler);

            await proxy.UnsubscribeAsync<ITransactionManagerEvents>(handler);

            

            throw new NotImplementedException();
        }

        public async Task CommitTransactionAsync(TransactionId transactionId)
        {
            throw new NotImplementedException();
        }

        public async Task<Order> GetOrderAsync(TransactionId transactionId)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateOrderAsync(TransactionId transactionId, Order order)
        {
            throw new NotImplementedException();
        }
    }

    internal class TransactionManagerEventsHandler : ITransactionManagerEvents
    {
        public void RollbackTransaction()
        {
            throw new NotImplementedException();
        }

        public void CommitTransaction()
        {
            throw new NotImplementedException();
        }
    }
}