using AutoMapper;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;
using WorkOrderApp.Exceptions;
using WorkOrderApp.Services.Interfaces;

namespace WorkOrderApp.Services
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public WorkOrderService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper  = mapper;
        }

        public async Task<bool> CreateAsync(CreateWorkOrderDto data)
        {
            await _context.GetByIdAsync<User>(data.CustomerId);
            await _context.GetByIdAsync<Location>(data.LocationId);

            if (data.AssignedToId is not null)
                await _context.GetByIdAsync<User>(data.AssignedToId);

            var entity = _mapper.Map<WorkOrder>(data);
            await _context.CreateAsync(entity);

            await LogAsync(
                entity.Id, data.AssignedById ?? entity.CustomerId,
                "Created", null, nameof(WorkOrderStatus.Open), null);

            if (data.AssignedToId is not null)
                await LogAsync(entity.Id, data.AssignedById ?? entity.CustomerId,
                    "Assigned", null, null, "Executor set on creation");

            return true;
        }

        public async Task<bool> UpdateAsync(UpdateWorkOrderDto data, string updatedById)
        {
            var existing = await _context.GetByIdAsync<WorkOrder>(data.Id);

            if (existing.Status == WorkOrderStatus.Done)
                throw new BadRequestException("Completed work orders cannot be modified");

            var oldStatus = existing.Status;

            if (data.Title        is not null) existing.Title        = data.Title;
            if (data.Description  is not null) existing.Description  = data.Description;
            if (data.AssignedToId is not null) existing.AssignedToId = data.AssignedToId;
            if (data.AssignedById is not null) existing.AssignedById = data.AssignedById;
            if (data.Status.HasValue)          existing.Status       = data.Status.Value;
            if (data.Priority.HasValue)        existing.Priority     = data.Priority.Value;
            if (data.EstimatedTime.HasValue)   existing.EstimatedTime = data.EstimatedTime.Value;
            if (data.DueDate.HasValue)         existing.DueDate      = data.DueDate.Value;
            if (data.StartDate.HasValue)       existing.StartDate    = data.StartDate.Value;

            await _context.UpdateAsync(existing);

            var statusChanged = data.Status.HasValue && data.Status.Value != oldStatus;
            var action  = statusChanged ? $"Status → {data.Status!.Value}" : "Updated";
            var logOld  = statusChanged ? oldStatus.ToString() : null;
            var logNew  = statusChanged ? data.Status!.Value.ToString() : null;

            await LogAsync(existing.Id, updatedById, action, logOld, logNew, data.Notes);

            return true;
        }

        public async Task<bool> AssignAsync(AssignWorkOrderDto data, string assignedById)
        {
            var existing = await _context.GetByIdAsync<WorkOrder>(data.Id);
            await _context.GetByIdAsync<User>(data.AssignedToId);

            existing.AssignedToId = data.AssignedToId;
            existing.AssignedById = assignedById;

            await _context.UpdateAsync(existing);
            await LogAsync(existing.Id, assignedById,
                "Assigned", null, null, $"Executor → {data.AssignedToId}");

            return true;
        }

        // ── Private log helper ────────────────────────────────────────────────

        private async Task LogAsync(
            string workOrderId, string userId,
            string action, string? oldStatus, string? newStatus, string? notes)
        {
            await _context.CreateAsync(new WorkOrderLog
            {
                WorkOrderId = workOrderId,
                UserId      = userId,
                Action      = action,
                OldStatus   = oldStatus,
                NewStatus   = newStatus,
                Notes       = notes
            });
        }
    }
}
