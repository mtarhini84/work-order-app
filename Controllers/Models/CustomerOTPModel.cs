using System.ComponentModel.DataAnnotations;

namespace WorkOrderApp.Controllers
{
    public class CustomerOTPModel
    {
        [Required]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
