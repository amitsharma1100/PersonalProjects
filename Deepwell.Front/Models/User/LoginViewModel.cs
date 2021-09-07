using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Deepwell.Common.Enum;
using Deepwell.Common.Helpers;

namespace Deepwell.Front.Models.User
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Log in with your e-mail address")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        [Required]        
        [Display(Name = "Location")]
        public InventoryLocation LocationSelected { get; set; }

        //public IEnumerable<SelectListItem> Locations
        //{
        //    get
        //    {
        //        return new List<SelectListItem>
        //        {
        //            new SelectListItem{ Text = "Location 1", Value = InventoryLocation.One.ToString()},
        //            new SelectListItem{ Text = "Location 2", Value = InventoryLocation.Two.ToString()},
        //        };
        //    }
        //}

        public SelectList Locations
        {
            get
            {
               return new SelectList(Utility.Locations, "Key", "Value");
            }
        }
    }
}