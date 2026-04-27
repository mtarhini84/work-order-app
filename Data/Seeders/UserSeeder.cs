using Microsoft.EntityFrameworkCore;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Entities;
using WorkOrderApp.Helpers.Utils;

namespace WorkOrderApp.Data.Seeders
{
    public class UserSeeder : IDataSeeder
    {
        public async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Users.CountAsync() > 1) return;

            var hash = PasswordUtils.HashPassword("Test1234");

            var users = new List<User>
            {
                new() { Name = "Omar Khalid",   Email = "operator1@app.com",  Role = "Operator", PasswordHash = hash, PhoneNumber = "+966501110001" },
                new() { Name = "Sara Nasser",   Email = "operator2@app.com",  Role = "Operator", PasswordHash = hash, PhoneNumber = "+966501110002" },
                new() { Name = "Ahmed Mansour", Email = "executor1@app.com",  Role = "Executor", PasswordHash = hash, PhoneNumber = "+966501110003" },
                new() { Name = "Lina Farouk",   Email = "executor2@app.com",  Role = "Executor", PasswordHash = hash, PhoneNumber = "+966501110004" },
                new() { Name = "Yusuf Al-Amin", Email = "executor3@app.com",  Role = "Executor", PasswordHash = hash, PhoneNumber = "+966501110005" },
                new() { Name = "Hana Ibrahim",  Email = "customer1@app.com",  Role = "Customer", PasswordHash = hash, PhoneNumber = "+966501110006" },
                new() { Name = "Tariq Saleem",  Email = "customer2@app.com",  Role = "Customer", PasswordHash = hash, PhoneNumber = "+966501110007" },
                new() { Name = "Nour Ziad",     Email = "customer3@app.com",  Role = "Customer", PasswordHash = hash, PhoneNumber = "+966501110008" },
            };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();
        }
    }
}
