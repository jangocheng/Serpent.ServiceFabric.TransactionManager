namespace TransactionManager
{
    using System;
    using System.Linq;
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
        private readonly IDeleteActor deleteActor;

        private TransactionSettings transactionSettings;

        /// <summary>
        ///     Initializes a new instance of TransactionManager
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        /// <param name="deleteActor">The delete actor interface</param>
        public TransactionManager(ActorService actorService, ActorId actorId, IDeleteActor deleteActor)
            : base(actorService, actorId)
        {
            this.deleteActor = deleteActor;
            this.transactionSettings = new TransactionSettings(this.ActorService.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config"));
        }

        public async Task<TransactionId> BeginTransactionAsync(TransactionProperties transactionProperties, CancellationToken cancellationToken)
        {
            var timeout = transactionProperties.TransactionTimeout ?? this.transactionSettings.DefaultTransactionTimeout;

            if (timeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(transactionProperties), "transactionProperties.TransactionTimeout must be equal to or greater than 0");
            }

            await this.StartTransactionTimeoutReminderAsync(timeout);

            await this.StateManager.AddStateAsync(StateConstants.TransactionEnd, timeout, cancellationToken);
            await this.StateManager.AddStateAsync(StateConstants.TransactionParticipants, TransactionParticipants.Empty, cancellationToken);
            await this.SetTransactionStateStatusAsync(TransactionStatus.Active, cancellationToken);

            return new TransactionId(this.GetActorId());
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            var status = await this.GetTransactionStateStatusAsync(cancellationToken);

            var actorId = this.GetActorId();

            if (status != TransactionStatus.Active)
            {
                var errorMessage = "Commit is only available when a transaction is in active state. Current state: " + status;
                ActorEventSource.Current.ActorMessage(this, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // I suppose loggin every successful transaction can generate quite a lot of data
            ActorEventSource.Current.ActorMessage(this, "Transaction commit started for transaction: " + actorId);
            try
            {
                await this.StartTransactionCommitTimeoutAsync();
                await this.StartTransactionCommitNotificationAsync();
                await this.SetTransactionStateStatusAsync(TransactionStatus.CommitStarted, cancellationToken);
            }
            catch (Exception exception)
            {
                ActorEventSource.Current.ActorMessage(this, "Initializing transaction commit failed for transaction: " + this.GetActorId() + ":" + exception.ToString());
                throw;
            }
        }

        public async Task<Interfaces.TransactionStatus> GetTransactionStatusAsync(CancellationToken cancellationToken)
        {
            var status = await this.GetTransactionStateStatusAsync(cancellationToken);
            return this.ConvertToPublicStatus(status);
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
                case ReminderConstants.MaximumTransactionTimeToLive:
                    await this.HandleTransactionTimeToLiveExpiredAsync();
                    break;
                case ReminderConstants.TransactionCommitNotification:
                    await this.HandleTransactionCommitNotificationAsync();
                    break;
                case ReminderConstants.TransactionCommitTimeout:
                    await this.HandleTransactionCommitTimeoutAsync();
                    break;
            }
        }

        private Task HandleTransactionCommitTimeoutAsync()
        {
            throw new NotImplementedException();
        }

        private Task HandleTransactionCommitNotificationAsync()
        {
            var events = this.GetEvent<ITransactionManagerEvents>();
            events.CommitTransaction();
            return Task.CompletedTask;
        }

        public async Task<TransactionParticipantId> RegisterTransactionParticipantAsync(CancellationToken cancellationToken)
        {
            var participantId = new TransactionParticipantId(Guid.NewGuid());
            await this.AddTransactionParticipantAsync(participantId, cancellationToken);
            return participantId;
        }

        public async Task ReportTransactionParticipantCommittedAsync(TransactionParticipantId transactionParticipantId, CancellationToken cancellationToken)
        {
            var participants = await this.GetTransactionParticipantsAsync(cancellationToken);
            participants.Replace(new TransactionParticipant(transactionParticipantId, TransactionParticipantStatus.Committed));
            await this.SetTransactionParticipantsAsync(participants, cancellationToken);

            if (participants.All(p => p.TransactionParticipantStatus == TransactionParticipantStatus.Committed))
            {
                await this.SetTransactionStateStatusAsync(TransactionStatus.Committed, cancellationToken);
                await this.StopTransactionCommitNotificationAsync();
                await this.StopTransactionTimeoutReminderAsync();
            }
        }

        public async Task ReportTransactionParticipantRolledBackAsync(TransactionParticipantId transactionParticipantId, CancellationToken cancellationToken)
        {
            var participants = await this.GetTransactionParticipantsAsync(cancellationToken);
            participants.Replace(new TransactionParticipant(transactionParticipantId, TransactionParticipantStatus.Rolledback));
            await this.SetTransactionParticipantsAsync(participants, cancellationToken);

            if (participants.All(p => p.TransactionParticipantStatus == TransactionParticipantStatus.Committed))
            {
                await this.SetTransactionStateStatusAsync(TransactionStatus.RollbackDone, cancellationToken);
                await this.StopTransactionRollbackNotificationAsync();
                await this.StopTransactionTimeoutReminderAsync();
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            var status = await this.GetTransactionStateStatusAsync(cancellationToken);

            var actorId = this.GetActorId();

            if (status != TransactionStatus.Active)
            {
                var errorMessage = "Rollback is only available when a transaction is in active state. Current state: " + status;
                ActorEventSource.Current.ActorMessage(this, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            ActorEventSource.Current.ActorMessage(this, "Transaction rollback started for transaction: " + actorId);
            try
            {
                await this.StartTransactionRollbackTimeoutAsync();
                await this.StartTransactionRollbackNotificationAsync();
                await this.SetTransactionStateStatusAsync(TransactionStatus.RollbackStarted, cancellationToken);
            }
            catch (Exception exception)
            {
                // TODO: add exception information
                ActorEventSource.Current.ActorMessage(this, "Transaction rollback failed for transaction: " + this.GetActorId());
                throw;
            }
        }

        public async Task UnregisterTransactionParticipantAsync(TransactionParticipantId transactionParticipantId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This method is called whenever an actor is activated.
        ///     An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Transaction created (but not started).");

            await this.StartTransactionTimeoutReminderAsync(this.transactionSettings.DefaultTransactionTimeout);
            await this.StartTransactionTimeToLiveReminderAsync();

            await this.SetTransactionStateStatusAsync(TransactionStatus.Created);
        }

        /// <summary>
        ///     Notifies all subscribers a rollback is requested
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        private Task HandleTransactionRollbackNotificationAsync()
        {
            var events = this.GetEvent<ITransactionManagerEvents>();
            events.RollbackTransaction();
            return Task.CompletedTask;
        }

        private async Task HandleTransactionRollbackTimeoutAsync()
        {
            var transactionId = new TransactionId(this.GetActorId());

            ActorEventSource.Current.ActorMessage(
                this,
                "Transaction rollback timed out: " + transactionId + ". The transaction will persist for a period of time to allow the participant to come online and rollback");

            // The transaction rollback timed out.. Stop sending notifications. 
            await this.StopTransactionRollbackNotificationAsync();

            // Report transaction participants that have not rolled back
            var participants = await this.GetTransactionParticipantsAsync();

            foreach (var participant in participants.Where(p => p.TransactionParticipantStatus != TransactionParticipantStatus.Rolledback))
            {
                ActorEventSource.Current.ActorMessage(
                    this,
                    "Transaction rollback: "
                    + transactionId
                    + ". Participant id: "
                    + participant.TransactionParticipantId
                    + " never reported back rollback. Last status: "
                    + participant.TransactionParticipantStatus);
            }
        }

        private async Task HandleTransactionTimedOutAsync()
        {
            var status = await this.GetTransactionStateStatusAsync();

            if (status == TransactionStatus.Active)
            {
                ActorEventSource.Current.ActorMessage(this, "Transaction timed out: " + this.GetActorId() + ". Initiating rollback.");

                await this.RollbackTransactionAsync(CancellationToken.None);
            }
            else
            {
                ActorEventSource.Current.ActorMessage(this, "HandleTransactionTimedOutAsync should never be called unless actor state is Active");
                throw new InvalidOperationException("HandleTransactionTimedOutAsync should never be called unless actor state is Active");
            }
        }

        /// <summary>
        ///     Event handler for time to live expired.
        ///     Signals to the garbage collector to delete the actor
        /// </summary>
        /// <returns></returns>
        private Task HandleTransactionTimeToLiveExpiredAsync()
        {
            this.deleteActor.DeleteActor(this.GetActorId());
            return Task.CompletedTask;
        }

        private async Task StartTransactionRollbackNotificationAsync()
        {
            await this.RegisterReminderAsync(
                ReminderConstants.TransactionRollbackNotification,
                null,
                TimeSpan.Zero,
                this.transactionSettings.TransactionRollbackNotificationInterval);
        }

        private async Task StartTransactionRollbackTimeoutAsync()
        {
            await this.RegisterReminderAsync(
                ReminderConstants.TransactionRollbackTimeout,
                null,
                this.transactionSettings.TransactionRollbackTimeout,
                TimeSpan.FromMilliseconds(-1));
        }

        private async Task StartTransactionTimeoutReminderAsync(TimeSpan timeout)
        {
            await this.RegisterReminderAsync(ReminderConstants.TransactionTimeout, null, timeout, TimeSpan.FromSeconds(10));
        }

        private async Task StartTransactionTimeToLiveReminderAsync()
        {
            await this.RegisterReminderAsync(ReminderConstants.MaximumTransactionTimeToLive, null, this.transactionSettings.MaximumTransactionTimeToLive, TimeSpan.FromMinutes(10));
        }

        private async Task StopTransactionRollbackNotificationAsync()
        {
            var reminder = this.GetReminder(ReminderConstants.TransactionRollbackNotification);
            await this.UnregisterReminderAsync(reminder);
        }
        private async Task StopTransactionCommitNotificationAsync()
        {
            var reminder = this.GetReminder(ReminderConstants.TransactionCommitNotification);
            await this.UnregisterReminderAsync(reminder);
        }

        private async Task StopTransactionTimeoutReminderAsync()
        {
            var reminder = this.GetReminder(ReminderConstants.TransactionTimeout);
            await this.UnregisterReminderAsync(reminder);
        }

        private async Task StartTransactionCommitNotificationAsync()
        {
            await this.RegisterReminderAsync(
                ReminderConstants.TransactionCommitNotification,
                null,
                TimeSpan.Zero,
                this.transactionSettings.TransactionCommitNotificationInterval);
        }

        private async Task StartTransactionCommitTimeoutAsync()
        {
            await this.RegisterReminderAsync(
                ReminderConstants.TransactionCommitTimeout,
                null,
                this.transactionSettings.TransactionCommitTimeout,
                TimeSpan.FromMilliseconds(-1));
        }
    }
}