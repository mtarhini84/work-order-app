using WorkOrderApp.Controllers;

namespace WorkOrderApp.Services.Interfaces
{
    public interface IPartService
    {
        Task<bool> CreateAsync(CreatePartDto data);
        Task<bool> UpdateAsync(UpdatePartDto data);
        Task<bool> DeleteAsync(string id);
        Task<PartDetails?> GetByQRCodeAsync(string qrCode);
    }
}
