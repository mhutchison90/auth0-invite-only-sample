using System.ComponentModel.DataAnnotations;

namespace Analystick.Web.Models
{
    public class UserActivationModel
    {
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public string UserToken
        {
            get;
            set;
        }
    }
}