using WorkOrderApp.Controllers;

namespace WorkOrderApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> CreateAsync(CreateUserDto data);
        Task<bool> UpdateAsync(UpdateUserDto data);
        Task<bool> UpdatePasswordAsync(UpdatePasswordModel data);
        Task<bool> MakeAdminAsync(string id);
        Task<AuthResult> LoginAsync(LoginModel data);
        Task<AuthResult> GetDetailsWithTokenAsync(string id);
    }
}
