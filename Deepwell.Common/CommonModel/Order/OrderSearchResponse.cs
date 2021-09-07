using System;

namespace Deepwell.Common.CommonModel.Order
{
    public class OrderSearchResponse
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int OrderStatus { get; set; }
        public int ProductId { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public int CustomerId { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public int LocationId { get; set; }
    }
}
