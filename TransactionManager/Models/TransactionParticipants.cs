namespace TransactionManager.Models
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    using global::TransactionManager.Interfaces;

    /// <summary>
    ///     The immutable <see cref="TransactionParticipants" /> type
    /// </summary>
    [DataContract(Name = "TransactionParticipants")]
    public class TransactionParticipants : IEnumerable<TransactionParticipant>
    {
        [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "TransactionParticipants", Order = 1)]
        private readonly TransactionParticipant[] transactionParticipants;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionParticipants" /> class.
        /// </summary>
        /// <param name="transactionParticipants">
        ///     The participants
        /// </param>
        public TransactionParticipants(IEnumerable<TransactionParticipant> transactionParticipants)
        {
            this.transactionParticipants = transactionParticipants.ToArray();
        }

        /// <summary>
        ///     Returns an empty <see cref="TransactionParticipants" /> instance
        /// </summary>
        public static TransactionParticipants Empty { get; } = new TransactionParticipants(Enumerable.Empty<TransactionParticipant>());

        /// <summary>
        ///     Adds a transaction participant id and returns a new instance of TransactionParticipants
        /// </summary>
        /// <param name="transactionParticipant">The transaction participant id to add</param>
        /// <returns>A new instance of <see cref="TransactionParticipants" /> with transactionParticipantId appended</returns>
        public TransactionParticipants Add(TransactionParticipant transactionParticipant)
        {
            return new TransactionParticipants(this.transactionParticipants.Append(transactionParticipant).ToArray());
        }

        public void Replace(TransactionParticipant transactionParticipant)
        {
            for (var i = 0; i < this.transactionParticipants.Length; i++)
            {
                var participant = this.transactionParticipants[i];
                if (participant.TransactionParticipantId.Equals(transactionParticipant.TransactionParticipantId))
                {
                    // This is OK being called from an actor, since it's thread safe already
                    this.transactionParticipants[i] = transactionParticipant;
                    break;
                }
            }
        }

        /// <summary>Determines whether a sequence contains a specified element by using the default equality comparer.</summary>
        /// <param name="transactionParticipantId">The transaction participant id to look for.</param>
        /// <returns>true if the transaction participants contains the specified transaction participant id; otherwise, false.</returns>
        public bool Contains(TransactionParticipantId transactionParticipantId)
        {
            return this.transactionParticipants.Any(tp => tp.TransactionParticipantId.Equals(transactionParticipantId));
        }

        public IEnumerator<TransactionParticipant> GetEnumerator()
        {
            return ((IEnumerable<TransactionParticipant>)this.transactionParticipants).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}