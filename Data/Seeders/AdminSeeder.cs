using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Entities;
using WorkOrderApp.Helpers.Utils;
using WorkOrderApp.Settings;

namespace WorkOrderApp.Data.Seeders
{
    public class AdminSeeder : IDataSeeder
    {
        private readonly AdminSeedSettings _settings;

        public AdminSeeder(IOptions<AdminSeedSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SeedAsync(ApplicationDbContext context)
        {
            var exists = await context.Users.AnyAsync(u => u.Role == _settings.Role);
            if (exists) return;

            var admin = new User
            {
                Name         = _settings.Name,
                Email        = _settings.Email,
                Role         = _settings.Role,
                PasswordHash = PasswordUtils.HashPassword(_settings.Password)
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
