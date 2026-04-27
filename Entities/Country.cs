using System.Text.Json.Serialization;

namespace WorkOrderApp.Entities
{
	public class Country : BaseEntity
	{
		public required string Name { get; set; }
		public required string IsoCode { get; set; }

		[JsonIgnore]
		public ICollection<City> Cities { get; set; } = new List<City>();
	}
}
