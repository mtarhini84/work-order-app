using WorkOrderApp.Controllers;

namespace WorkOrderApp.Services.Interfaces
{
    public interface ILocationService
    {
        Task<bool> CreateAsync(CreateLocationDto data);
        Task<bool> UpdateAsync(UpdateLocationDto data);
        Task<bool> AssignUserAsync(AssignUserToLocationDto data);
        Task<bool> RemoveUserAsync(AssignUserToLocationDto data);
        Task<IEnumerable<UserDetails>> GetUsersAsync(string locationId);
        Task<IEnumerable<LocationDetails>> GetUserLocationsAsync(string userId);
    }
}
