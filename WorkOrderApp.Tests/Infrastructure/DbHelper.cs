using Microsoft.Extensions.DependencyInjection;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Entities;
using WorkOrderApp.Helpers.Utils;

namespace WorkOrderApp.Tests.Infrastructure
{
    /// <summary>
    /// Seeds the in-memory database with the minimum data required for tests.
    /// Returns entity IDs so test classes can reference them in requests.
    /// </summary>
    public static class DbHelper
    {
        public static async Task<SeedData> SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var hash = PasswordUtils.HashPassword("Test1234");

            var admin = new User    { Name = "Seed Admin",    Email = "seed.admin@test.com",    Role = "Admin",    PasswordHash = hash };
            var op    = new User    { Name = "Seed Operator", Email = "seed.operator@test.com", Role = "Operator", PasswordHash = hash };
            var exec  = new User    { Name = "Seed Executor", Email = "seed.executor@test.com", Role = "Executor", PasswordHash = hash };
            var cust  = new User    { Name = "Seed Customer", Email = "seed.customer@test.com", Role = "Customer", PasswordHash = hash };
            var loc   = new Location { Name = "Test Location", Address = "1 Test St" };

            db.Users.AddRange(admin, op, exec, cust);
            db.Locations.Add(loc);
            await db.SaveChangesAsync();

            var request = new Request
            {
                Title         = "Seed Request",
                LocationId    = loc.Id,
                RequestedById = cust.Id,
                Status        = RequestStatus.Pending,
                Priority      = Priority.Medium,
            };
            db.Requests.Add(request);
            await db.SaveChangesAsync();

            var workOrder = new WorkOrder
            {
                Title        = "Seed WorkOrder",
                CustomerId   = cust.Id,
                LocationId   = loc.Id,
                AssignedToId = exec.Id,
                AssignedById = op.Id,
                Status       = WorkOrderStatus.Open,
                Priority     = Priority.Medium,
            };
            db.WorkOrders.Add(workOrder);
            await db.SaveChangesAsync();

            var cost = new Cost
            {
                WorkOrderId = workOrder.Id,
                UserId      = exec.Id,
                Name        = "Seed Cost",
                Amount      = 100m,
                Category    = "Labour",
            };
            var part = new Part
            {
                WorkOrderId = workOrder.Id,
                UserId      = exec.Id,
                Name        = "Seed Part",
                UnitCost    = 50m,
                Count       = 2,
                QRCode      = "QR-TEST-001",
            };
            var attachment = new Attachment
            {
                WorkOrderId = workOrder.Id,
                Url         = "https://storage.example.com/test/file.jpg",
                FileName    = "file.jpg",
                ContentType = "image/jpeg",
            };

            db.Costs.Add(cost);
            db.Parts.Add(part);
            db.Attachments.Add(attachment);
            await db.SaveChangesAsync();

            return new SeedData(admin.Id, op.Id, exec.Id, cust.Id,
                                loc.Id, request.Id, workOrder.Id,
                                cost.Id, part.Id, attachment.Id);
        }
    }

    public record SeedData(
        string AdminId,
        string OperatorId,
        string ExecutorId,
        string CustomerId,
        string LocationId,
        string RequestId,
        string WorkOrderId,
        string CostId,
        string PartId,
        string AttachmentId);
}
