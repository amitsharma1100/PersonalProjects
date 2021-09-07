using System.ComponentModel.DataAnnotations;

namespace Deepwell.Front.Models.User
{
    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}