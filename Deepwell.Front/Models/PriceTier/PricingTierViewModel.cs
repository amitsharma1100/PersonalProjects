using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Deepwell.Front.Models.PriceTier
{
    public class PricingTierViewModel
    {
        [Display(Name = "Tier ID")]
        public int TierId { get; set; }

        public bool IsEditMode => TierId > 0;
        public bool IsActive { get; set; }
        public string PageTitle => TierId > 0
            ? "Edit Price Tier"
            : "Add New Tier";

        [Display(Name = "Tier Name")]
        [Required(ErrorMessage = "Tier name is required")]
        public string TierName { get; set; }

        [Display(Name = "Active")]
        public string IsActiveDisplay { get; set; }
        public bool IsAdministrator { get; set; }
        public string EditUrl { get; set; }
        public IEnumerable<SelectListItem> Products { get; set; }
        public IEnumerable<PriceTierProduct> SelectedProducts { get; set; }
        public IEnumerable<string> SelectedProductIds { get; set; }
    }
}