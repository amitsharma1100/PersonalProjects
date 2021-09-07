using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Deepwell.Front.Models.User
{
    public class SearchViewModel
    {
        [Display(Name = "Employee ID")]
        public int EmployeeId { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Active")]
        public string IsActive { get; set; }

        [Display(Name = "Active")]
        public List<SelectListItem> IsActiveOptions { get; set; }

        public bool IsAdministrator { get; set; }

        public string ValidationMessage { get; set; }
    }
}