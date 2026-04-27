using System.ComponentModel.DataAnnotations;

namespace WorkOrderApp.Controllers
{
	public class LoginModel
	{
		[Required]
		public string Identifier { get; set; } = string.Empty;

		[Required]
		[StringLength(256)]
		public string Password { get; set; } = string.Empty;
	}
}
