namespace TransactionManager.Models
{
    using System.Runtime.Serialization;

    using global::TransactionManager.Interfaces;

    [DataContract(Name = "TransactionParticipant")]
    public class TransactionParticipant
    {
        public TransactionParticipant(TransactionParticipantId transactionParticipantId, TransactionParticipantStatus transactionParticipantStatus = TransactionParticipantStatus.Added)
        {
            this.TransactionParticipantId = transactionParticipantId;
            this.TransactionParticipantStatus = transactionParticipantStatus;
        }
        
        [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "TransactionParticipantId", Order = 1)]
        public TransactionParticipantId TransactionParticipantId { get; }

        [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "TransactionParticipantStatus", Order = 2)]
        public TransactionParticipantStatus TransactionParticipantStatus { get; }
    }
}