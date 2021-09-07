using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Deepwell.Common.Enum;

namespace Deepwell.Front.Models.Product
{
    public class ProductInventoryViewModel
    {       
        public InventoryLocation LocationId { get; set; }

        public string LocationName { get; set; }

        [Required(ErrorMessage = "Please provide Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Only non negative value can be entered for Inventory.")]           
        public int Quantity { get; set; }

        public int QuantityIncreasedBy { get; set; }

        public int QuantityDecreasedBy { get; set; }

        public int QuantityTransferred { get; set; }

        public InventoryLocation ToLocationId { get; set; }

        public string Remarks { get; set; }

        public int InitialInventory { get; set; }

        public string InventoryLastUpdatedDate { get; set; }

        public bool IsCurrentLocation { get; set; }
    }
}