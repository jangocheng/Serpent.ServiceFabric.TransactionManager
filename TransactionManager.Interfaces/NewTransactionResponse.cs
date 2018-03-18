namespace TransactionManager.Interfaces
{
    using System;

    public struct NewTransactionResponse
    {
        public NewTransactionResponse(TransactionId transactionId, Guid commitKey)
        {
            this.TransactionId = transactionId;
            this.CommitKey = commitKey;
        }

        public TransactionId TransactionId { get; }

        public Guid CommitKey { get; set; }
    }
}