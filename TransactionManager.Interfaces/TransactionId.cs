namespace TransactionManager.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    using Microsoft.ServiceFabric.Actors;

    [DataContract(Name = "TransactionId")]
    public sealed class TransactionId
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false, Name = "GuidId", Order = 3)]
        private readonly Guid guidId;

        [DataMember(IsRequired = true, Name = "Kind", Order = 1)]
        private readonly ActorIdKind kind;

        [DataMember(EmitDefaultValue = false, IsRequired = false, Name = "LongId", Order = 2)]
        private readonly long longId;

        [DataMember(EmitDefaultValue = false, IsRequired = false, Name = "StringId", Order = 4)]
        private readonly string stringId;

        private volatile string stringRepresentation;

        public TransactionId(ActorId actorId)
        {
            switch (actorId.Kind)
            {
                case ActorIdKind.Long:
                    this.kind = ActorIdKind.Long;
                    this.longId = actorId.GetLongId();
                    break;
                case ActorIdKind.Guid:
                    this.kind = ActorIdKind.Long;
                    this.guidId = actorId.GetGuidId();
                    break;
                case ActorIdKind.String:
                    this.kind = ActorIdKind.Long;
                    this.stringId = actorId.GetStringId();
                    break;
                default:
                    throw new InvalidOperationException($"he ActorIdKind value {(object)this.kind} is invalid");
            }
        }

        public ActorIdKind Kind => this.kind;

        public byte[] GetBytes()
        {
            var binaryKind = BitConverter.GetBytes((int)this.kind);

            switch (this.kind)
            {
                case ActorIdKind.Long:
                    return MergeToArray(binaryKind, BitConverter.GetBytes(this.longId));
                case ActorIdKind.Guid:
                    return MergeToArray(binaryKind, this.guidId.ToByteArray());
                case ActorIdKind.String:
                    return MergeToArray(binaryKind, BitConverter.GetBytes(this.stringId.Length), Encoding.UTF8.GetBytes(this.stringId));
                default:
                    throw new InvalidOperationException($"he ActorIdKind value {(object)this.kind} is invalid");
            }
        }

        public Guid GetGuidId()
        {
            if (this.kind == ActorIdKind.Guid)
            {
                return this.guidId;
            }

            throw new InvalidOperationException($"he ActorIdKind value {(object)this.kind} is invalid");
        }

        public override int GetHashCode()
        {
            switch (this.kind)
            {
                case ActorIdKind.Long:
                    return this.longId.GetHashCode();
                case ActorIdKind.Guid:
                    return this.guidId.GetHashCode();
                case ActorIdKind.String:
                    return this.stringId.GetHashCode();
                default:
                    throw new InvalidOperationException($"he ActorIdKind value {(object)this.kind} is invalid");
            }
        }

        public long GetLongId()
        {
            if (this.kind == ActorIdKind.Long)
            {
                return this.longId;
            }

            throw new InvalidOperationException($"he ActorIdKind value {(object)this.kind} is invalid");
        }

        public string GetStringId()
        {
            if (this.kind == ActorIdKind.String)
            {
                return this.stringId;
            }

            throw new InvalidOperationException($"he ActorIdKind value {(object)this.kind} is invalid");
        }

        public ActorId ToActorId()
        {
            switch (this.kind)
            {
                case ActorIdKind.Long:
                    return new ActorId(this.longId);
                case ActorIdKind.Guid:
                    return new ActorId(this.guidId);
                case ActorIdKind.String:
                    return new ActorId(this.stringId);
                default:
                    throw new InvalidOperationException($"he ActorIdKind value {(object)this.kind} is invalid");
            }
        }

        public override string ToString()
        {
            if (this.stringRepresentation != null)
            {
                return this.stringRepresentation;
            }

            var str = string.Empty;
            switch (this.kind)
            {
                case ActorIdKind.Long:
                    str = this.longId.ToString(CultureInfo.InvariantCulture);
                    break;
                case ActorIdKind.Guid:
                    str = this.guidId.ToString();
                    break;
                case ActorIdKind.String:
                    str = this.stringId;
                    break;
                default:
                    throw new InvalidOperationException($"he ActorIdKind value {(object)this.kind} is invalid");
            }

            this.stringRepresentation = str;
            return str;
        }

        private static T[] MergeToArray<T>(params IEnumerable<T>[] collections)
        {
            return collections?.SelectMany(c => c).ToArray();
        }
    }
}