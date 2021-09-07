using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Deepwell.Common.Extensions;

namespace Deepwell.Front.Models.Order
{
    public class OrderSummaryViewModel
    {
        //public decimal TaxableItemsTotal { get; set; }
        //public string TaxableItemsTotalToDisplay => this.TaxableItemsTotal.FormatCurrency();

        //public decimal NonTaxableItemsTotal { get; set; }
        //public string NonTaxableItemsTotalToDisplay => this.NonTaxableItemsTotal.FormatCurrency();

        //public string OrderTotal => (this.TaxableItemsTotal + this.NonTaxableItemsTotal).FormatCurrency();

        public decimal OrderTotal { get; set; }
        public string OrderTotalToDisplay => this.OrderTotal.FormatCurrency();
    }
}