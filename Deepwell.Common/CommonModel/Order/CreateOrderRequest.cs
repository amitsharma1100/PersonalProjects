using Deepwell.Common.CommonModel.Customer;
using System;
using System.Collections.Generic;

namespace Deepwell.Common.CommonModel.Order
{
    public class CreateOrderRequest
    {
        public int OrderId { get; set; }
        public int PriceTierId { get; set; }
        public DateTime OrderDate { get; set; }
        public int OrderStatus { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string Phone { get; set; }
        public Address BillingAddress { get; set; }
        public Address ShippingAddress { get; set; }
        public IEnumerable<OrderItemDetail> Items { get; set; }
        public decimal TaxableTotal { get; set; }
        public decimal NonTaxableTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public int PlacedById { get; set; }
    }
}
