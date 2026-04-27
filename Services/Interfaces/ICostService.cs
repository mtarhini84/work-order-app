using WorkOrderApp.Controllers;

namespace WorkOrderApp.Services.Interfaces
{
    public interface ICostService
    {
        Task<bool> CreateAsync(CreateCostDto data);
        Task<bool> UpdateAsync(UpdateCostDto data);
        Task<bool> DeleteAsync(string id);
    }
}
