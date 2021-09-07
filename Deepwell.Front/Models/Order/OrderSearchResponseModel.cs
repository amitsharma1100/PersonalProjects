using Deepwell.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Deepwell.Front.Models.Order
{
    public class OrderSearchResponseModel
    {
        [Display(Name = "Order No.")]
        public int OrderId { get; set; }

        [Display(Name = "Order date")]
        public string OrderDate { get; set; }

        [Display(Name = "Order Status")]
        public string OrderStatus { get; set; }

        public int ProductId { get; set; }

        [Display(Name = "Product Number")]
        public string ProductNumber { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        public int CustomerId { get; set; }

        [Display(Name = "Customer Number")]
        public string CustomerNumber { get; set; }

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        public int LocationId { get; set; }

        public string EditButtonText => this.LocationId == SessionHelper.LocationId
            ? "Edit"
            : "View";
    }
}