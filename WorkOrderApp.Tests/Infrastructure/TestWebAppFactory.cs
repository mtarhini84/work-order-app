using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkOrderApp.AppDbContext;

namespace WorkOrderApp.Tests.Infrastructure
{
    /// <summary>
    /// Boots the real application but replaces Postgres with an in-memory database
    /// so tests run without any external dependencies.
    /// </summary>
    public class TestWebAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove the real Postgres registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Add in-memory database unique per factory instance.
                services.AddDbContext<ApplicationDbContext>(opts =>
                    opts.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                        .EnableSensitiveDataLogging());

                // Ensure schema created + seed reference data.
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}
