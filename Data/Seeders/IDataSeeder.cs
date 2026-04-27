using WorkOrderApp.AppDbContext;

namespace WorkOrderApp.Data.Seeders
{
    public interface IDataSeeder
    {
        Task SeedAsync(ApplicationDbContext context);
    }
}
