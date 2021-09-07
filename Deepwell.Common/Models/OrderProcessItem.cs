namespace Deepwell.Common.Models
{
    public class OrderProcessItem
    {        
        public int OrderDetailId { get; set; }

        public int OrderProcessId { get; set; }

        public int Quantity { get; set; }

        public int LocationId { get; set; }

        public int ProductId { get; set; }

        public int InventoryId { get; set; }

        public int QuantityReturned { get; set; }
    }
}