namespace TransactionManager.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    using global::TransactionManager.Interfaces;

    /// <summary>
    ///     The immutable <see cref="TransactionParticipants" /> type
    /// </summary>
    [DataContract(Name = "TransactionParticipants")]
    public class TransactionParticipants
    {
        [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "Id", Order = 1)]
        private readonly TransactionParticipantId[] transactionParticipantIds;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionParticipants" /> class.
        /// </summary>
        /// <param name="transactionParticipantIds">
        ///     The participant ids
        /// </param>
        public TransactionParticipants(IEnumerable<TransactionParticipantId> transactionParticipantIds)
        {
            this.transactionParticipantIds = transactionParticipantIds.ToArray();
        }

        /// <summary>
        ///     Returns an empty <see cref="TransactionParticipants" /> instance
        /// </summary>
        public static TransactionParticipants Empty { get; } = new TransactionParticipants(Enumerable.Empty<TransactionParticipantId>());

        /// <summary>
        ///     Adds a transaction participant id and returns a new instance of TransactionParticipants
        /// </summary>
        /// <param name="transactionParticipantId">The transaction participant id to add</param>
        /// <returns>A new instance of <see cref="TransactionParticipants" /> with transactionParticipantId appended</returns>
        public TransactionParticipants Add(TransactionParticipantId transactionParticipantId)
        {
            return new TransactionParticipants(this.transactionParticipantIds.Append(transactionParticipantId).ToArray());
        }

        /// <summary>Determines whether a sequence contains a specified element by using the default equality comparer.</summary>
        /// <param name="transactionParticipantId">The transaction participant id to look for.</param>
        /// <returns>true if the transaction participants contains the specified transaction participant id; otherwise, false.</returns>
        public bool Contains(TransactionParticipantId transactionParticipantId)
        {
            return this.transactionParticipantIds.Contains(transactionParticipantId);
        }
    }
}