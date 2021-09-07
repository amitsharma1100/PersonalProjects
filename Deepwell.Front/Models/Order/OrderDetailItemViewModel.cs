namespace Deepwell.Front.Models.Order
{
    using System.ComponentModel.DataAnnotations;
    using Deepwell.Common.Enum;
    using Deepwell.Common.Extensions;
    using Deepwell.Common.Helpers;
    using Deepwell.Data;
    using Deepwell.Front.Helpers;

    public class OrderDetailItemViewModel
    {
        public int OrderDetailId { get; set; }

        public bool IsEditMode => this.OrderDetailId > 0;

        public int LineNumber { get; set; }

        public OrderStatus Status { get; set; }

        public string LocationLabel => this.LocationId == 1
             ? "Location 1"
             : "Location 2";

        public int LocationId { get; set; }

        public int InvoiceId { get; set; }

        public int CreditMemoId { get; set; }

        public int ProductId { get; set; }
        public string ProductNumber { get; set; }

        public string ProductName { get; set; }

        public decimal ListPrice { get; set; }
        public string PriceToDisplay => this.SellPrice.FormatCurrency();

        public decimal SellPrice => this.IsEditMode
            ? this.SoldPrice
            : this.TierPrice > 0
                ? this.TierPrice
                : this.ListPrice;
        public string SellPriceToDisplay => this.SellPrice.FormatCurrency();

        public decimal TierPrice => OrderItemsHelper.GetProductPriceFromTier(this.ProductId);

        public bool IsDisplayTierPriceSymbol => this.SellPrice != this.ListPrice && this.ListPrice > 0;

        public string ItemTotal => this.OrderDetailId > 0
            ? (this.Quantity * this.SoldPrice).FormatCurrency()
            : (this.Quantity * this.SellPrice).FormatCurrency();

        // Price at which item was sold.
        public decimal SoldPrice { get; set; }

        public bool IsTaxable { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Only numeric value can be entered for Inventory.")]
        public int Quantity { get; set; }

        public int QuantityAvailable { get; set; }

        //public int QuantityAvailable => this.productInventory.IsNotNull() 
        //    ? this.productInventory.Quantity.Value 
        //    : 0;        

        public int QuantityReturned { get; set; }
        public int MaxReturnableQuantity => Quantity - QuantityReturned;

        public bool IsDisplayRemoveButton => this.OrderDetailId == 0 && this.IsLocationSame;

        //public bool IsDisplayShipButton => this.OrderDetailId > 0 && (this.Status == OrderStatus.Active) && this.IsLocationSame;

        public bool IsDisplayShipButton => this.OrderDetailId > 0 
            && (this.Status == OrderStatus.Active || this.Status == OrderStatus.Invoiced) 
            && this.IsLocationSame;

        public bool IsDisplayInvoiceButton => this.OrderDetailId > 0 
            && (this.Status == OrderStatus.Active || this.Status == OrderStatus.Shipped) 
            && this.IsLocationSame;

        public bool IsDisplayReturnButton => this.OrderDetailId > 0 
            && (this.Status == OrderStatus.Shipped || this.Status == OrderStatus.Invoiced || this.Status == OrderStatus.ShippedAndInvoiced) 
            && this.IsLocationSame 
            && this.MaxReturnableQuantity > 0;

        public bool IsDisplayCancelButton => this.OrderDetailId > 0 && (this.Status == OrderStatus.Active) && this.IsLocationSame;

        public bool IsDisplayQuantityAvailable => this.Status == OrderStatus.Active && this.IsLocationSame;

        public bool IsQuantityEditable => this.Status == OrderStatus.Active && this.IsLocationSame;

        public bool IsLocationSame => this.LocationId == SessionHelper.LocationId;

        public string UnitOfMeasure { get; set; }

        public int OrderProcessId { get; set; }

        public ProductInventory Inventory { get; set; }

        public int InventoryId => this.Inventory.IsNotNull()
           ? this.Inventory.InventoryId
           : 0;

        public bool IsDisplayCheckBox => this.Status == OrderStatus.Active 
            || this.Status == OrderStatus.Shipped 
            || this.Status == OrderStatus.Invoiced;

        public bool IsInvoiced { get; set; }
    }
}