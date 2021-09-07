using Deepwell.Common.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Deepwell.Front.Models.Customer
{
    public class CustomerSearchModel
    {
        public int CustomerID { get; set; }

        [Display(Name = "Customer Number:")]
        public string CustomerNumber { get; set; }

        [Display(Name = "Customer Name:")]
        public string Name { get; set; }

        [Display(Name = "Customer Status:")]
        public CustomerStatus Status { get; set; }

        [Display(Name = "Customer Type:")]
        public CustomerType Type { get; set; }

        public bool IsAdministrator { get; set; }

        [Display(Name = "Customer Taxable:")]
        public List<SelectListItem> Taxable { get; set; }
    }
}