using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;

namespace Deepwell.Front.Models.Order
{
    public class OrderSearchRequestModel
    {
        [DisplayName("Order Number:")]
        public int OrderId { get; set; }

        [DisplayName("Order Date From:")]
        public string OrderDateFrom { get; set; }

        [DisplayName("To:")]
        public string OrderDateTo { get; set; }

        public IEnumerable<SelectListItem> OrderStatusList { get; set; }

        [DisplayName("Order Status:")]
        public int OrderStatus { get; set; }

        public int ProductId { get; set; }

        [DisplayName("Product Number:")]
        public string ProductNumber { get; set; }

        [DisplayName("Product Name:")]
        public string ProductName { get; set; }

        [DisplayName("Customer Number:")]
        public string CustomerNumber { get; set; }

        [DisplayName("Customer Name:")]
        public string CustomerName { get; set; }
    }
}