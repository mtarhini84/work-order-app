using WorkOrderApp.Controllers;

namespace WorkOrderApp.Services.Interfaces
{
    public interface IWorkOrderService
    {
        Task<bool> CreateAsync(CreateWorkOrderDto data);
        Task<bool> UpdateAsync(UpdateWorkOrderDto data, string updatedById);
        Task<bool> AssignAsync(AssignWorkOrderDto data, string assignedById);
    }
}
