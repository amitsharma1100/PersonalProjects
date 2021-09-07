using System;
using System.ComponentModel.DataAnnotations;
using Deepwell.Common.Enum;

namespace Deepwell.Front.Models.Inventory
{
    public class InventoryLogListViewModel
    {
        [Display(Name = "Date")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Location")]
        public string Location { get; set; }

        [Display(Name = "Inventory Change")]
        public int QuantityAffected { get; set; }

        [Display(Name = "Change Type")]
        public string ChangeType { get; set; }
             
        public string Remarks { get; set; }

        public string UserName { get; set; }
    }
}