using WorkOrderApp.AppDbContext;

namespace WorkOrderApp.Data.Seeders
{
    public static class SeederRunner
    {
        public static async Task RunAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context  = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var seeders  = scope.ServiceProvider.GetServices<IDataSeeder>();

            foreach (var seeder in seeders)
                await seeder.SeedAsync(context);
        }
    }
}
