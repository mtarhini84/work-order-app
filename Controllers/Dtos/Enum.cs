using WorkOrderApp.Entities;

namespace WorkOrderApp.Controllers
{
	public class CreateEnumDto
	{
		public string Name { get; set; } = null!;
		public string? Description { get; set; } = null!;
	}

	public class UpdateEnumDto
	{
		public required string Id { get; set; }
		public string Name { get; set; } = null!;
		public string? Description { get; set; } = null!;
	}

	public class EnumDetails : BaseDetails
	{
		public string Name { get; set; } = null!;
		public string? Description { get; set; } = null!;
	}

	// PropertyType-specific DTOs
	public class CreatePropertyTypeDto : CreateEnumDto
	{
		public bool IsCommercial { get; set; }
	}

	public class UpdatePropertyTypeDto : UpdateEnumDto
	{
		public bool IsCommercial { get; set; }
	}

	public class PropertyTypeDetails : EnumDetails
	{
		public bool IsCommercial { get; set; }
	}
}
