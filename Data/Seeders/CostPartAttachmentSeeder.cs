using Microsoft.EntityFrameworkCore;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Entities;

namespace WorkOrderApp.Data.Seeders
{
    public class CostPartAttachmentSeeder : IDataSeeder
    {
        public async Task SeedAsync(ApplicationDbContext context)
        {
            bool hasCosts       = await context.Costs.AnyAsync();
            bool hasParts       = await context.Parts.AnyAsync();
            bool hasAttachments = await context.Attachments.AnyAsync();

            if (hasCosts && hasParts && hasAttachments) return;

            var executorIds  = await context.Users.Where(u => u.Role == "Executor").Select(u => u.Id).ToListAsync();
            var workOrderIds = await context.WorkOrders.Select(wo => wo.Id).ToListAsync();
            var requestIds   = await context.Requests.Select(r => r.Id).ToListAsync();

            if (executorIds.Count == 0 || workOrderIds.Count == 0) return;

            string exId1 = executorIds[0];
            string exId2 = executorIds.Count > 1 ? executorIds[1] : exId1;
            string woId1 = workOrderIds[0];
            string woId2 = workOrderIds.Count > 1 ? workOrderIds[1] : woId1;
            string woId3 = workOrderIds.Count > 2 ? workOrderIds[2] : woId1;

            // ── Costs ─────────────────────────────────────────────────────────
            if (!hasCosts)
            {
                var costs = new List<Cost>
                {
                    new() { WorkOrderId = woId1, UserId = exId1, Name = "Refrigerant R-410A top-up", Amount = 280m,  Category = "Materials", Description = "1 kg R-410A refrigerant" },
                    new() { WorkOrderId = woId1, UserId = exId1, Name = "Labour — HVAC inspection",  Amount = 150m,  Category = "Labour",    Description = "2 hours at 75/hr" },
                    new() { WorkOrderId = woId2, UserId = exId2, Name = "Electronic lock unit",      Amount = 450m,  Category = "Materials", Description = "Smart lock model LX-200" },
                    new() { WorkOrderId = woId2, UserId = exId2, Name = "Labour — lock installation", Amount = 100m, Category = "Labour",    Description = "1 hour installation" },
                    new() { WorkOrderId = woId3, UserId = exId1, Name = "Transport to site",         Amount = 60m,   Category = "Transport", Description = "Round trip vehicle cost" },
                    new() { WorkOrderId = woId1, UserId = exId2, Name = "Drainage tray replacement", Amount = 85m,  Category = "Materials", Description = "Plastic condensate tray" },
                };
                context.Costs.AddRange(costs);
            }

            // ── Parts ─────────────────────────────────────────────────────────
            if (!hasParts)
            {
                var parts = new List<Part>
                {
                    new() { WorkOrderId = woId1, UserId = exId1, Name = "Air filter 20x20", UnitCost = 35m,  Count = 2, QRCode = "QR-AF-001", Description = "Standard HVAC air filter" },
                    new() { WorkOrderId = woId1, UserId = exId1, Name = "Capacitor 45μF",   UnitCost = 22m,  Count = 1, QRCode = "QR-CP-045", Description = "Start capacitor for compressor" },
                    new() { WorkOrderId = woId2, UserId = exId2, Name = "Door sensor strip", UnitCost = 18m, Count = 1, QRCode = "QR-DS-007", Description = "Magnetic contact strip" },
                    new() { WorkOrderId = woId2, UserId = exId2, Name = "Cat6 cable (5m)",   UnitCost = 12m, Count = 3, QRCode = "QR-CAT6-5", Description = "Ethernet cable for lock controller" },
                    new() { WorkOrderId = woId3, UserId = exId1, Name = "Glass sealant tube", UnitCost = 15m, Count = 2, QRCode = "QR-GS-002", Description = "Weather-resistant silicone" },
                    new() { WorkOrderId = woId1, UserId = exId2, Name = "Copper pipe 1/4\"",  UnitCost = 45m, Count = 1, QRCode = "QR-PIP-14", Description = "Refrigerant line segment" },
                };
                context.Parts.AddRange(parts);
            }

            // ── Attachments ───────────────────────────────────────────────────
            if (!hasAttachments)
            {
                string? reqId1 = requestIds.Count > 0 ? requestIds[0] : null;
                string? reqId2 = requestIds.Count > 1 ? requestIds[1] : reqId1;

                var attachments = new List<Attachment>
                {
                    new() { WorkOrderId = woId1, Url = "https://storage.example.com/wo/photo-hvac-before.jpg",  FileName = "hvac-before.jpg",  ContentType = "image/jpeg" },
                    new() { WorkOrderId = woId1, Url = "https://storage.example.com/wo/photo-hvac-after.jpg",   FileName = "hvac-after.jpg",   ContentType = "image/jpeg" },
                    new() { WorkOrderId = woId2, Url = "https://storage.example.com/wo/lock-diagram.pdf",       FileName = "lock-diagram.pdf", ContentType = "application/pdf" },
                    new() { RequestId  = reqId1, Url = "https://storage.example.com/req/ac-complaint.jpg",      FileName = "ac-complaint.jpg", ContentType = "image/jpeg" },
                    new() { RequestId  = reqId2, Url = "https://storage.example.com/req/lock-video.mp4",        FileName = "lock-video.mp4",   ContentType = "video/mp4" },
                };
                context.Attachments.AddRange(attachments);
            }

            await context.SaveChangesAsync();
        }
    }
}
