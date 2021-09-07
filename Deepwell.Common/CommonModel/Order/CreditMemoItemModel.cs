namespace Deepwell.Common.CommonModel.Order
{
    public class CreditMemoItemModel
    {
        public int OrderProcessId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantityToReturn { get; set; }
        public int OwnerId { get; set; }
        public int OrderDetailId { get; set; }
        public int LocationId { get; set; }
        public int Quantity { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Price { get; set; }
    }
}
