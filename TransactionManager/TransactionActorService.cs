namespace TransactionManager
{
    using System;
    using System.Fabric;
    using System.Threading;

    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    internal class TransactionActorService : ActorService
    {
        private readonly TransactionManagerActorGarbageCollector transactionManagerActorGarbageCollector;

        public TransactionActorService(
            StatefulServiceContext context,
            ActorTypeInformation actorTypeInfo,
            Func<TransactionActorService, ActorId, IDeleteActor, TransactionManager> actorFactory = null,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null,
            ActorServiceSettings settings = null)
            : base(
                context,
                actorTypeInfo,
                (service, id) =>
                    {
                        var actorService = (TransactionActorService)service;
                        return actorFactory?.Invoke(actorService, id, actorService.DeleteActorService);
                    },
                stateManagerFactory,
                stateProvider,
                settings)
        {
            this.transactionManagerActorGarbageCollector = new TransactionManagerActorGarbageCollector(this.StateProvider, TimeSpan.FromSeconds(10), CancellationToken.None);
        }

        internal IDeleteActor DeleteActorService => this.transactionManagerActorGarbageCollector;
    }
}