using AutoMapper;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;
using WorkOrderApp.Exceptions;
using WorkOrderApp.Services.Interfaces;

namespace WorkOrderApp.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AttachmentService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper  = mapper;
        }

        public async Task<bool> CreateAsync(CreateAttachmentDto data)
        {
            if (data.RequestId is null && data.WorkOrderId is null)
                throw new BadRequestException("An attachment must belong to a request or a work order");

            if (data.RequestId    is not null) await _context.GetByIdAsync<Request>(data.RequestId);
            if (data.WorkOrderId  is not null) await _context.GetByIdAsync<WorkOrder>(data.WorkOrderId);

            var entity = _mapper.Map<Attachment>(data);
            var result = await _context.CreateAsync(entity);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _context.GetByIdAsync<Attachment>(id);
            await _context.DeleteAsync(entity);
            return true;
        }
    }
}
