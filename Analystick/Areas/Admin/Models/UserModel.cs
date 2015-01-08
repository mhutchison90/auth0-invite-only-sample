using System.ComponentModel.DataAnnotations;

namespace Analystick.Web.Areas.Admin.Models
{
    public class UserModel
    {
        public string UserId
        {
            get;
            set;
        }

        [Required]
        public string GivenName
        {
            get;
            set;
        }

        [Required]
        public string FamilyName
        {
            get;
            set;
        }

        [Required]
        public string Email
        {
            get;
            set;
        }
    }
}