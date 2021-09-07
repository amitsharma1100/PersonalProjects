using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Deepwell.Front.Models.Customer
{
    public class CustomerViewModel
    {
        public int CustomerId { get; set; }
        public bool IsEdit => this.CustomerId > 0;

        [Display(Name = "Customer Number:")]
        [MaxLength(50, ErrorMessage = "Maximum length allowed is 50.")]
        [Required(ErrorMessage = "Please enter Customer Number.")]
        public string CustomerNumber { get; set; }

        [Display(Name = "Customer Name:")]
        [Required(ErrorMessage = "Please enter Customer Name.")]
        public string Name { get; set; }

        [Display(Name = "Customer Status:")]
        public string CustomerStatus { get; set; }

        public IEnumerable<SelectListItem> CustomerStatusOptions { get; set; }

        [Display(Name = "Customer Type:")]
        public string CustomerType { get; set; }

        public IEnumerable<SelectListItem> CustomerTypeOptions { get; set; }

        [Display(Name = "Price Setting:")]
        public string CustomerPricingSettings { get; set; }

        public IEnumerable<SelectListItem> CustomerPricingSettingsOptions { get; set; }

        [Required]
        [Display(Name = "Taxable:")]
        public bool IsTaxable { get; set; }

        [Display(Name = "Email:")]       
        [DataType(DataType.EmailAddress, ErrorMessage = "Please enter valid Email Address.")]
        [Required(ErrorMessage = "Please enter Email Address.")]
        public string Email { get; set; }

        [Display(Name = "Phone:")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        [Required(ErrorMessage = "Please enter Phone number.")]
        public string Phone { get; set; }

        public CustomerAddress Address { get; set; }

        [Display(Name = "Billing Type:")]
        public string BillingType { get; set; }

        public IEnumerable<SelectListItem> BillingTypeOptions { get; set; }
    }
}