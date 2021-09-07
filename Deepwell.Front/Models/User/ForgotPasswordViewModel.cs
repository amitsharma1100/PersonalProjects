using System.ComponentModel.DataAnnotations;

namespace Deepwell.Front.Models.User
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}