using System.ComponentModel.DataAnnotations;

namespace Deepwell.Front.Models.User
{
    public class StaffUserViewModel
    {
        [Required(ErrorMessage = "Please enter First Name")]
        [Display(Name = "First Name:")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter Last Name")]
        [Display(Name = "Last Name:")]
        public string LastName { get; set; }

        [Display(Name = "Email:")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Please enter valid Email Address.")]
        [Required(ErrorMessage = "Please enter Email Address")]
        public string Email { get; set; }

        [Display(Name = "Employee ID:")]
        public int EmployeeId { get; set; }

        [Required]
        [Display(Name = "Active:")]
        public bool IsActive { get; set; }

        [Required]
        [Display(Name = "Administrator:")]
        public bool IsAdministrator { get; set; }
    }
}