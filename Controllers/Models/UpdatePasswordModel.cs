using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WorkOrderApp.Controllers
{
	public class UpdatePasswordModel
	{
		[JsonIgnore]
		public string? Id { get; set; }

		[Required]
		[StringLength(256)]
		public string OldPassword { get; set; } = string.Empty;

		[Required]
		[StringLength(256)]
		public string NewPassword { get; set; } = string.Empty;
	}
}
