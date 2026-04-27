using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;
using WorkOrderApp.Exceptions;
using WorkOrderApp.Services.Interfaces;

namespace WorkOrderApp.Services
{
    public class RequestService : IRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RequestService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper  = mapper;
        }

        public async Task<bool> CreateAsync(CreateRequestDto data, string requestedById)
        {
            await _context.GetByIdAsync<Location>(data.LocationId);

            var entity = _mapper.Map<Request>(data);
            entity.RequestedById = requestedById;
            entity.Status        = RequestStatus.Pending;

            await _context.CreateAsync(entity);

            await LogRequestAsync(
                entity.Id, requestedById,
                "Created", null, nameof(RequestStatus.Pending), null);

            return true;
        }

        public async Task<bool> UpdateAsync(UpdateRequestDto data, string updatedById)
        {
            var existing = await _context.GetByIdAsync<Request>(data.Id);

            if (existing.Status != RequestStatus.Pending)
                throw new BadRequestException("Only pending requests can be edited");

            if (data.Title       is not null) existing.Title       = data.Title;
            if (data.Description is not null) existing.Description = data.Description;
            if (data.Priority.HasValue)       existing.Priority    = data.Priority.Value;
            if (data.Picture     is not null) existing.Picture     = data.Picture;
            if (data.ContactInfo is not null) existing.ContactInfo = data.ContactInfo;

            await _context.UpdateAsync(existing);

            await LogRequestAsync(existing.Id, updatedById, "Updated", null, null, null);

            return true;
        }

        public async Task<bool> ApproveAsync(ApproveRequestDto data, string approvedById)
        {
            var existing = await _context.GetByIdAsync<Request>(data.Id);

            if (existing.Status != RequestStatus.Pending)
                throw new BadRequestException("Only pending requests can be approved");

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var oldStatus = existing.Status.ToString();
                existing.Status = RequestStatus.Approved;
                await _context.UpdateAsync(existing);

                await LogRequestAsync(
                    existing.Id, approvedById,
                    "Approved", oldStatus, nameof(RequestStatus.Approved), data.Notes);

                // Convert the request into a work order
                var workOrder = new WorkOrder
                {
                    Title         = existing.Title,
                    Description   = existing.Description,
                    CustomerId    = existing.RequestedById,
                    LocationId    = existing.LocationId,
                    RequestId     = existing.Id,
                    Priority      = existing.Priority,
                    Status        = WorkOrderStatus.Open,
                    AssignedToId  = data.AssignedToId,
                    AssignedById  = approvedById,
                    DueDate       = data.DueDate,
                    EstimatedTime = data.EstimatedTime
                };

                await _context.CreateAsync(workOrder);

                var woNote = $"Created from Request #{existing.Number}";
                await LogWorkOrderAsync(workOrder.Id, approvedById,
                    "Created", null, nameof(WorkOrderStatus.Open), woNote);

                if (data.AssignedToId is not null)
                    await LogWorkOrderAsync(workOrder.Id, approvedById,
                        "Assigned", null, null, $"Executor set on approval");

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeclineAsync(DeclineRequestDto data, string declinedById)
        {
            var existing = await _context.GetByIdAsync<Request>(data.Id);

            if (existing.Status != RequestStatus.Pending)
                throw new BadRequestException("Only pending requests can be declined");

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var oldStatus = existing.Status.ToString();
                existing.Status        = RequestStatus.Declined;
                existing.DeclineReason = data.DeclineReason;
                await _context.UpdateAsync(existing);

                await LogRequestAsync(
                    existing.Id, declinedById,
                    "Declined", oldStatus, nameof(RequestStatus.Declined),
                    data.Notes ?? data.DeclineReason);

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> MarkDoneAsync(string id, string userId)
        {
            var existing = await _context.GetByIdAsync<Request>(id);

            if (existing.Status == RequestStatus.Declined)
                throw new BadRequestException("A declined request cannot be marked done");

            var oldStatus = existing.Status.ToString();
            existing.Status = RequestStatus.Done;
            await _context.UpdateAsync(existing);

            await LogRequestAsync(
                existing.Id, userId,
                "Marked Done", oldStatus, nameof(RequestStatus.Done), null);

            return true;
        }

        // ── Private log helpers ───────────────────────────────────────────────

        private async Task LogRequestAsync(
            string requestId, string userId,
            string action, string? oldStatus, string? newStatus, string? notes)
        {
            await _context.CreateAsync(new RequestLog
            {
                RequestId = requestId,
                UserId    = userId,
                Action    = action,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Notes     = notes
            });
        }

        private async Task LogWorkOrderAsync(
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
