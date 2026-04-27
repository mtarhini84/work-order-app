using Microsoft.EntityFrameworkCore;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Entities;

namespace WorkOrderApp.Data.Seeders
{
    public class LocationSeeder : IDataSeeder
    {
        public async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Locations.AnyAsync()) return;

            var locations = new List<Location>
            {
                new() { Name = "Main Warehouse",     Address = "12 Industrial Ave, Block A",    Description = "Primary storage and dispatch hub" },
                new() { Name = "North Branch Office", Address = "88 Commerce St, Suite 4",      Description = "Regional admin and customer support" },
                new() { Name = "Workshop Floor B2",  Address = "12 Industrial Ave, Block B, L2", Description = "Heavy machinery maintenance area" },
                new() { Name = "Site Alpha",         Address = "Desert Rd, Km 14, Plot 7",      Description = "Remote field site — outdoor equipment" },
                new() { Name = "City Service Centre", Address = "3 Central Plaza, Ground Floor", Description = "Walk-in customer service and light repairs" },
                new() { Name = "Depot Gamma",        Address = "Port Zone, Gate 9",             Description = "Container yard and vehicle storage" },
                new() { Name = "HQ Building",        Address = "1 Corporate Blvd, Floor 10",    Description = "Executive offices and server room" },
            };

            context.Locations.AddRange(locations);
            await context.SaveChangesAsync();
        }
    }
}
