using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Dynamic;

namespace WorkOrderApp.Entities
{
	public abstract class BaseEntity
	{
		[Key]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[Required]
		public bool Active { get; set; } = true;

		public DateTimeOffset CreatedAt { get; set; }
		
		public DateTimeOffset UpdatedAt { get; set; }
	}
}
