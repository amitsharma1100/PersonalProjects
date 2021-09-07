using Deepwell.Common.Enum;
using System;

namespace Deepwell.Common.CommonModel.Order
{
    public class OrderItemDetail
    {
        public int OrderDetailId { get; set; }
        public int LineNumber { get; set; }
        public OrderStatus Status { get; set; }
        public int LocationId { get; set; }
        public int ProductId { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public decimal SellPrice { get; set; }
        public decimal ListPrice { get; set; }
        public bool IsTaxable { get; set; }
        public int Quantity { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
