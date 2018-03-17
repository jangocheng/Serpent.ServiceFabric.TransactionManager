namespace TransactionManager.Interfaces
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Name = "TransactionParticipantId")]
    public struct TransactionParticipantId : IEquatable<TransactionParticipantId>
    {
        [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "Id", Order = 1)]
        private readonly Guid id;

        private string stringRepresentation;

        public TransactionParticipantId(Guid id)
        {
            this.id = id;
            this.stringRepresentation = null;
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }

        public override string ToString()
        {
            if (this.stringRepresentation != null)
            {
                return this.stringRepresentation;
            }

            var str = this.id.ToString();
            this.stringRepresentation = str;
            return str;
        }

        public bool Equals(TransactionParticipantId other)
        {
            return this.id.Equals(other.id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is TransactionParticipantId && Equals((TransactionParticipantId)obj);
        }
    }
}