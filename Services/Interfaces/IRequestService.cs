using WorkOrderApp.Controllers;

namespace WorkOrderApp.Services.Interfaces
{
    public interface IRequestService
    {
        Task<bool> CreateAsync(CreateRequestDto data, string requestedById);
        Task<bool> UpdateAsync(UpdateRequestDto data, string updatedById);
        Task<bool> ApproveAsync(ApproveRequestDto data, string approvedById);
        Task<bool> DeclineAsync(DeclineRequestDto data, string declinedById);
        Task<bool> MarkDoneAsync(string id, string userId);
    }
}
