using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Deepwell.Common.Enum;
using Deepwell.Common.Helpers;

namespace Deepwell.Front.Models.Inventory
{
    public class InventoryLogViewModel
    {
        public SelectList Locations
        {
            get
            {
                return new SelectList(Utility.Locations, "Key", "Value");
            }
        }

        public SelectList InventoryChangeTypes
        {
            get
            {
                return new SelectList(Utility.InventoryChangeTypes, "Key", "Value");
            }
        }

        [Display(Name = "Change Type")]
        public InventoryChangeType ChangeTypeSelected { get; set; }

        [Display(Name = "Location")]
        public InventoryLocation LocationSelected { get; set; }

        [Display(Name = "Date From")]
        public DateTime? FromDate { get; set; }

        [Display(Name = "Date To")]
        public DateTime? ToDate { get; set; }

        public IEnumerable<InventoryLogListViewModel> InventoryLogs { get; set; }
    }
}