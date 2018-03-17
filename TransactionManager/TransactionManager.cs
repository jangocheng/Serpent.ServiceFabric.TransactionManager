﻿namespace TransactionManager
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using global::TransactionManager.Configuration;
    using global::TransactionManager.Interfaces;
    using global::TransactionManager.Models;

    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    [StatePersistence(StatePersistence.Persisted)]
    internal class TransactionManager : Actor, ITransactionManager, IRemindable
    {
        private TransactionSettings transactionSettings;

        /// <summary>
        ///     Initializes a new instance of TransactionManager
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public TransactionManager(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            this.transactionSettings = new TransactionSettings(this.ActorService.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config"));
        }

        public async Task<TransactionId> BeginTransactionAsync(TransactionProperties transactionProperties, CancellationToken cancellationToken)
        {
            var timeout = transactionProperties.TransactionTimeout ?? this.transactionSettings.DefaultTransactionTimeout;

            if (timeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(transactionProperties), "transactionProperties.TransactionTimeout must be equal to or greater than 0");
            }

            await this.StateManager.AddStateAsync(StateConstants.TransactionEnd, timeout, cancellationToken);
            await this.StateManager.AddStateAsync(StateConstants.TransactionParticipants, TransactionParticipants.Empty, cancellationToken);

            await this.StartTransactionTimeoutReminderAsync(timeout);

            await this.SetTransactionStatusAsync(TransactionStatus.Active, cancellationToken);

            return new TransactionId(this.GetActorId());
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            switch (reminderName)
            {
                case ReminderConstants.TransactionTimeout:
                    await this.HandleTransactionTimedOutAsync();
                    break;
                case ReminderConstants.TransactionRollbackNotification:
                    await this.HandleTransactionRollbackNotificationAsync();
                    break;
                case ReminderConstants.TransactionRollbackTimeout:
                    await this.HandleTransactionRollbackTimeoutAsync();
                    break;
            }
        }

        private Task HandleTransactionRollbackTimeoutAsync()
        {
            throw new NotImplementedException();
        }

        private Task HandleTransactionRollbackNotificationAsync()
        {
            throw new NotImplementedException();
        }

        private async Task HandleTransactionTimedOutAsync()
        {
            var status = await this.GetTransactionStatusAsync();

            if (status == TransactionStatus.Active)
            {
                ActorEventSource.Current.ActorMessage(this, "Transaction timed out: " + this.GetActorId() + ". Initiating rollback.");

                await this.RollbackTransactionAsync(CancellationToken.None);
            }
        }

        public async Task<TransactionParticipantId> RegisterTransactionParticipantAsync(CancellationToken cancellationToken)
        {
            var participantId = new TransactionParticipantId(Guid.NewGuid());
            await this.AddOrUpdateTransactionParticipantAsync(participantId, cancellationToken);
            return participantId;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, "Transaction rollback started for transaction: " + this.GetActorId());
            try
            {
                await this.StartTransactionRollbackTimeoutAsync();
                await this.StartTransactionRollbackNotificationAsync();



                var events = this.GetEvent<ITransactionManagerEvents>();


                throw new NotImplementedException();
            }
            catch (Exception exception)
            {
                // TODO: add exception information
                ActorEventSource.Current.ActorMessage(this, "Transaction rollback failed for transaction: " + this.GetActorId());
                throw;
            }
        }



        /// <summary>
        ///     This method is called whenever an actor is activated.
        ///     An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Transaction activated.");

            return this.SetTransactionStatusAsync(TransactionStatus.Created);
        }

        private async Task StartTransactionTimeoutReminderAsync(TimeSpan timeout)
        {
            await this.RegisterReminderAsync(ReminderConstants.TransactionTimeout, null, timeout, TimeSpan.FromMilliseconds(-1));
        }

        private async Task StartTransactionRollbackTimeoutAsync()
        {
            await this.RegisterReminderAsync(ReminderConstants.TransactionRollbackTimeout, null, this.transactionSettings.TransactionRollbackTimeout, TimeSpan.FromMilliseconds(-1));
        }

        private async Task StartTransactionRollbackNotificationAsync()
        {
            await this.RegisterReminderAsync(ReminderConstants.TransactionRollbackNotification, null, this.transactionSettings.TransactionRollbackNotificationInterval, this.transactionSettings.TransactionRollbackNotificationInterval);
        }

    }
}