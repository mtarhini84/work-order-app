using WorkOrderApp.Controllers;

namespace WorkOrderApp.Services.Interfaces
{
    public interface IAttachmentService
    {
        Task<bool> CreateAsync(CreateAttachmentDto data);
        Task<bool> DeleteAsync(string id);
    }
}
