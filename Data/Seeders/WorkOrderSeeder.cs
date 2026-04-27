using Microsoft.EntityFrameworkCore;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Entities;

namespace WorkOrderApp.Data.Seeders
{
    public class WorkOrderSeeder : IDataSeeder
    {
        public async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.WorkOrders.AnyAsync()) return;

            var customerIds  = await context.Users.Where(u => u.Role == "Customer").Select(u => u.Id).ToListAsync();
            var executorIds  = await context.Users.Where(u => u.Role == "Executor").Select(u => u.Id).ToListAsync();
            var operatorIds  = await context.Users.Where(u => u.Role == "Operator").Select(u => u.Id).ToListAsync();
            var locationIds  = await context.Locations.Select(l => l.Id).ToListAsync();
            var approvedReqs = await context.Requests
                .Where(r => r.Status == RequestStatus.Approved)
                .Select(r => r.Id)
                .ToListAsync();

            if (customerIds.Count == 0 || locationIds.Count == 0) return;

            string? executorId  = executorIds.Count  > 0 ? executorIds[0]  : null;
            string? operatorId  = operatorIds.Count  > 0 ? operatorIds[0]  : null;
            string? executor2Id = executorIds.Count  > 1 ? executorIds[1]  : executorId;

            var now = DateTimeOffset.UtcNow;

            var workOrders = new List<WorkOrder>
            {
                new()
                {
                    Title         = "HVAC repair — Room 204",
                    Description   = "Inspect and repair AC unit. Check refrigerant level and drainage tray.",
                    CustomerId    = customerIds[0],
                    LocationId    = locationIds[0],
                    RequestId     = approvedReqs.Count > 0 ? approvedReqs[0] : null,
                    AssignedToId  = executorId,
                    AssignedById  = operatorId,
                    Status        = WorkOrderStatus.InProgress,
                    Priority      = Priority.High,
                    EstimatedTime = 120,
                    DueDate       = now.AddDays(1),
                    StartDate     = now,
                },
                new()
                {
                    Title         = "Door lock replacement — Server Room",
                    Description   = "Replace electronic lock unit with new model. Test remote access after installation.",
                    CustomerId    = customerIds.Count > 1 ? customerIds[1] : customerIds[0],
                    LocationId    = locationIds.Count > 1 ? locationIds[1] : locationIds[0],
                    RequestId     = approvedReqs.Count > 1 ? approvedReqs[1] : null,
                    AssignedToId  = executor2Id,
                    AssignedById  = operatorId,
                    Status        = WorkOrderStatus.Open,
                    Priority      = Priority.High,
                    EstimatedTime = 90,
                    DueDate       = now.AddDays(2),
                },
                new()
                {
                    Title         = "Window glass replacement — North wing",
                    Description   = "On hold pending glass supplier delivery.",
                    CustomerId    = customerIds.Count > 1 ? customerIds[1] : customerIds[0],
                    LocationId    = locationIds.Count > 1 ? locationIds[1] : locationIds[0],
                    AssignedToId  = executorId,
                    AssignedById  = operatorId,
                    Status        = WorkOrderStatus.OnHold,
                    Priority      = Priority.Medium,
                    EstimatedTime = 180,
                    DueDate       = now.AddDays(5),
                },
                new()
                {
                    Title         = "Lighting inspection — Corridor C",
                    Description   = "Check wiring, replace faulty ballasts, test all corridor lights.",
                    CustomerId    = customerIds[0],
                    LocationId    = locationIds.Count > 2 ? locationIds[2] : locationIds[0],
                    AssignedToId  = executor2Id,
                    AssignedById  = operatorId,
                    Status        = WorkOrderStatus.Done,
                    Priority      = Priority.Low,
                    EstimatedTime = 60,
                    StartDate     = now.AddDays(-3),
                    DueDate       = now.AddDays(-1),
                },
                new()
                {
                    Title         = "Plumbing — Washroom B1 sink leak",
                    Description   = "Identify source of leak, replace washer or pipe section as required.",
                    CustomerId    = customerIds[0],
                    LocationId    = locationIds[0],
                    AssignedToId  = executorId,
                    Status        = WorkOrderStatus.Open,
                    Priority      = Priority.Medium,
                    EstimatedTime = 45,
                    DueDate       = now.AddDays(3),
                },
            };

            context.WorkOrders.AddRange(workOrders);
            await context.SaveChangesAsync();
        }
    }
}
