using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Deepwell.Front.Models.Customer
{
    public class CustomerAddress
    {
        [Display(Name = "Well Name:")]
        [Required(ErrorMessage = "Please enter WellName")]
        public string WellName { get; set; }

        [Display(Name = "County:")]
        [Required(ErrorMessage = "Please enter County.")]
        public string County { get; set; }

        [Required(ErrorMessage = "Please select State.")]
        public string StateId { get; set; }

        [Display(Name = "State:")]
        public IEnumerable<SelectListItem> States { get; set; }

        [Display(Name = "Zip Code:")]
        [DataType(DataType.PostalCode)]
        [RegularExpression(@"^[0-9]{5}(?:-[0-9]{4})?$", ErrorMessage = "Not a valid Zip Code")]
        [Required(ErrorMessage = "Please enter Zip Code.")]
        public string Zipcode { get; set; }

        [Display(Name = "City:")]
        [Required(ErrorMessage = "Please enter City.")]
        public string City { get; set; }
    }
}