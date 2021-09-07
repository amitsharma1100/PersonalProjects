using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Deepwell.Front.Helpers;

namespace Deepwell.Front.Models.Product
{
    public class ProductSearchViewModel
    {
        public int ProductId { get; set; }

        [Display(Name = "Product Number:")]
        public string ProductNumber { get; set; }

        [Display(Name = "Product Name:")]
        public string ProductName { get; set; }

        [Display(Name = "Taxable:")]
        public List<SelectListItem> Taxable { get; set; }

        [Display(Name = "Product Type:")]
        public List<SelectListItem> ProductType { get; set; }

        [Display(Name = "Active:")]
        public List<SelectListItem> IsActiveOptions => Utility.ActiveOptions;

        public bool IsAdministrator { get; set; }
    }
}