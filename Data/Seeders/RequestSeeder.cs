using Microsoft.EntityFrameworkCore;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Entities;

namespace WorkOrderApp.Data.Seeders
{
    public class RequestSeeder : IDataSeeder
    {
        public async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Requests.AnyAsync()) return;

            var customerIds  = await context.Users.Where(u => u.Role == "Customer").Select(u => u.Id).ToListAsync();
            var locationIds  = await context.Locations.Select(l => l.Id).ToListAsync();

            if (customerIds.Count == 0 || locationIds.Count == 0) return;

            var requests = new List<Request>
            {
                new()
                {
                    Title         = "AC unit not cooling — Room 204",
                    Description   = "The air conditioning unit in room 204 stopped cooling. Noticed water dripping from the unit.",
                    LocationId    = locationIds[0],
                    RequestedById = customerIds[0],
                    Status        = RequestStatus.Pending,
                    Priority      = Priority.High,
                    ContactInfo   = "Call ext. 204",
                },
                new()
                {
                    Title         = "Broken door lock — Server Room",
                    Description   = "The electronic lock on the server room door is malfunctioning and cannot be unlocked remotely.",
                    LocationId    = locationIds.Count > 1 ? locationIds[1] : locationIds[0],
                    RequestedById = customerIds.Count > 1 ? customerIds[1] : customerIds[0],
                    Status        = RequestStatus.Approved,
                    Priority      = Priority.High,
                    ContactInfo   = "Contact IT dept.",
                },
                new()
                {
                    Title         = "Leaking pipe in washroom B1",
                    Description   = "Water leaking under sink in the ground-floor washroom.",
                    LocationId    = locationIds[0],
                    RequestedById = customerIds[0],
                    Status        = RequestStatus.Pending,
                    Priority      = Priority.Medium,
                },
                new()
                {
                    Title         = "Elevator out of service",
                    Description   = "Elevator 2 has been stuck on floor 3 since morning. Maintenance company notified but not arrived.",
                    LocationId    = locationIds.Count > 2 ? locationIds[2] : locationIds[0],
                    RequestedById = customerIds.Count > 1 ? customerIds[1] : customerIds[0],
                    Status        = RequestStatus.Declined,
                    Priority      = Priority.High,
                    DeclineReason = "Handled by contracted elevator company — outside internal scope.",
                },
                new()
                {
                    Title         = "Flickering lights in corridor C",
                    Description   = "Three ceiling lights in corridor C flicker intermittently. Possible wiring issue.",
                    LocationId    = locationIds.Count > 1 ? locationIds[1] : locationIds[0],
                    RequestedById = customerIds[0],
                    Status        = RequestStatus.Done,
                    Priority      = Priority.Low,
                },
                new()
                {
                    Title         = "Generator maintenance overdue",
                    Description   = "Backup generator has not been serviced in 6 months. Requesting scheduled maintenance.",
                    LocationId    = locationIds.Count > 3 ? locationIds[3] : locationIds[0],
                    RequestedById = customerIds.Count > 2 ? customerIds[2] : customerIds[0],
                    Status        = RequestStatus.Pending,
                    Priority      = Priority.Medium,
                },
                new()
                {
                    Title         = "Broken window — North wing",
                    Description   = "Window pane cracked due to recent storm. Security risk at night.",
                    LocationId    = locationIds.Count > 1 ? locationIds[1] : locationIds[0],
                    RequestedById = customerIds.Count > 1 ? customerIds[1] : customerIds[0],
                    Status        = RequestStatus.Approved,
                    Priority      = Priority.High,
                },
                new()
                {
                    Title         = "Pest control request — Cafeteria",
                    Description   = "Cockroaches spotted near the food storage area. Immediate pest control needed.",
                    LocationId    = locationIds[0],
                    RequestedById = customerIds[0],
                    Status        = RequestStatus.Pending,
                    Priority      = Priority.High,
                },
            };

            context.Requests.AddRange(requests);
            await context.SaveChangesAsync();
        }
    }
}
