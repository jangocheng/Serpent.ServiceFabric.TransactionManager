namespace TransactionManager
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    internal class TransactionManagerActorGarbageCollector : IDeleteActor
    {
        private readonly IActorStateProvider actorStateProvider;

        private readonly TimeSpan scanInterval;

        private readonly CancellationToken cancellationToken;

        private readonly ConcurrentDictionary<ActorId, bool> actorsToRemove = new ConcurrentDictionary<ActorId, bool>();

        public TransactionManagerActorGarbageCollector(IActorStateProvider actorStateProvider, TimeSpan scanInterval, CancellationToken cancellationToken)
        {
            this.actorStateProvider = actorStateProvider;
            this.scanInterval = scanInterval;
            this.cancellationToken = cancellationToken;

            this.StartScanning();
        }

        private void StartScanning()
        {
            Task.Run(this.GarbageCollectorWorkerAsync, this.cancellationToken);
        }

        private async Task GarbageCollectorWorkerAsync()
        {
            do
            {
                await Task.Delay(this.scanInterval, this.cancellationToken);
                await this.CollectGarbageAsync();
            }
            while (!this.cancellationToken.IsCancellationRequested);
        }

        private async Task CollectGarbageAsync()
        {
            try
            {
                foreach (var actorId in this.actorsToRemove.Keys)
                {
                    try
                    {
                        await this.actorStateProvider.RemoveActorAsync(actorId, this.cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ActorEventSource.Current.Message("Error deleting actor: " + e.ToString());
                    }

                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                ActorEventSource.Current.Message("Actor GC failed: " + e.ToString());
            }
        }

        public void DeleteActor(ActorId actorId)
        {
            this.actorsToRemove.TryAdd(actorId, default(bool));
        }
    }
}