namespace Deepwell.Front.Models.Order
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;
    using Deepwell.Common.Extensions;
    using Deepwell.Common.Helpers;
    using Deepwell.Front.Models.Customer;

    public class OrderDetailViewModel
    {
        [DisplayName("Order Number:")]
        public int OrderId { get; set; }

        public int NewOrderId { get; set; }

        public bool IsEditMode => this.OrderId > 0;

        [DisplayName("Order Date:")]
        public DateTime OrderDate { get; set; }

        [DisplayName("Order Date:")]
        public string FormattedOrderDate => this.OrderDate.ToString("MM/dd/yyyy hh:mm tt");

        public SelectList OrderStatusList { get; set; }

        [DisplayName("Order Status:")]
        public int OrderStatus { get; set; }

        public IEnumerable<SelectListItem> CustomersList { get; set; }
        public IEnumerable<SelectListItem> PriceTierList { get; set; }

        [DisplayName("Customer:")]
        [Required(ErrorMessage = "Customer is required.")]
        public int CustomerId { get; set; }

        [DisplayName("Price Tier:")]
        public int TierId { get; set; }

        [DisplayName("Invoices:")]
        public int InvoiceId { get; set; }
        public IEnumerable<SelectListItem> InvoicesList { get; set; }
        public bool IsInvoicesListVisible => this.InvoicesList.IsNotNull() && this.InvoicesList.Count() > 0;

        [DisplayName("Shipments:")]
        public int ShipmentId { get; set; }
        public IEnumerable<SelectListItem> ShipmentList { get; set; }
        public bool IsShipmentListVisible => this.ShipmentList.IsNotNull() && this.ShipmentList.Count() > 0;

        [DisplayName("Credit Memos:")]
        public int CreditMemoId { get; set; }
        public IEnumerable<SelectListItem> CreditMemoList { get; set; }
        public bool IsCreditMemoListVisible => this.CreditMemoList.IsNotNull() && this.CreditMemoList.Count() > 0;

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        public CustomerAddress BillingAddress { get; set; }

        public CustomerAddress ShippingAddress { get; set; }

        public bool IsBillingAndShippinAddressSame => this.BillingAddress.IsNotNull()
            && this.BillingAddress.WellName.Equals(this.ShippingAddress.WellName)
            && this.BillingAddress.County.Equals(this.ShippingAddress.County)
            && this.BillingAddress.City.Equals(this.ShippingAddress.City)
            && this.BillingAddress.StateId.Equals(this.ShippingAddress.StateId)
            && this.BillingAddress.Zipcode.Equals(this.ShippingAddress.Zipcode);

        public IEnumerable<OrderDetailItemViewModel> Items { get; set; }

        public OrderSummaryViewModel OrderSummaryInfo
        {
            get
            {
                return new OrderSummaryViewModel
                {
                    OrderTotal = this.Items.IsNotNull()
                    ? this.Items
                    .Where(it => it.Status.Equals(Common.Enum.OrderStatus.Invoiced) || it.Status.Equals(Common.Enum.OrderStatus.Shipped) || it.Status.Equals(Common.Enum.OrderStatus.ShippedAndInvoiced) || it.Status.Equals(Common.Enum.OrderStatus.Active))
                    .Select(i => i.Quantity * i.SellPrice).Sum()
                    : 0
                };
            }
        }

        public bool IsDisplayInvoiceButton =>
            this.IsEditMode == true && this.Items.IsNotNull() && this.Items.Any(i => i.Status == Common.Enum.OrderStatus.Shipped);

        public bool IsDisplayLastInvoiceButton =>
            this.IsEditMode == true && this.Items.IsNotNull() && this.Items.Any(i => i.Status == Common.Enum.OrderStatus.Invoiced);

        public bool IsDisplayPriceTierList => this.PriceTierList.IsNotNull() && this.PriceTierList.Count() > 1 // checking 1 as first option is "Select Tier"
            ? this.IsEditMode
                ? this.TierId > 0
                : true
            : false;

        public bool IsDisplayAddProductsButton => this.IsEditMode == false || this.Items.IsNotNull() && this.Items.All(i => i.LocationId == SessionHelper.LocationId);

        public bool IsOrderEditable =>
            this.IsEditMode == false
            || (this.IsEditMode == true && this.Items.IsNotNull() && this.Items.All(i => i.Status == Common.Enum.OrderStatus.Active));

        public bool IsOrderHeaderEditable => this.IsEditMode == false
            || (this.IsEditMode == true && this.Items.IsNotNull() && this.Items.All(i => i.Status == Common.Enum.OrderStatus.Active));

        public string PageTitle => this.IsEditMode ? "Edit Order" : "Add New Order";

        public bool IsTaxable { get; set; }
    }
}