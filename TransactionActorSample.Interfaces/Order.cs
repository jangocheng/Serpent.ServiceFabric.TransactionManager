namespace TransactionActorSample.Interfaces
{
    public class Order
    {
        public string OrderNumber { get; set; }

        public OrderLine[] OrderLines { get; set; }
    }
}