using System;

namespace Deepwell.Common.CommonModel.Order
{
    public class OrderSearchRequest
    {
        public int OrderId { get; set; }
        public DateTime OrderDateFrom { get; set; }
        public DateTime OrderDateTo { get; set; }
        public int OrderStatus { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public int Page { get; set; } = 1;
    }
}
