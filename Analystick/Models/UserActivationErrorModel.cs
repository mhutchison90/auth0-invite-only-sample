namespace Analystick.Web.Models
{
    public class UserActivationErrorModel
    {
        public string Message { get; set; }
        public UserActivationErrorModel(string message)
        {
            Message = message;
        }
    }
}