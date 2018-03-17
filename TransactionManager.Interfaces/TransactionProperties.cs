namespace TransactionManager.Interfaces
{
    using System;

    public struct TransactionProperties
    {
        public TimeSpan? TransactionTimeout { get; set; }
    }
}