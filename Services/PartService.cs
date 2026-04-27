using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;
using WorkOrderApp.Services.Interfaces;

namespace WorkOrderApp.Services
{
    public class PartService : IPartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PartService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper  = mapper;
        }

        public async Task<bool> CreateAsync(CreatePartDto data)
        {
            await _context.GetByIdAsync<WorkOrder>(data.WorkOrderId);
            await _context.GetByIdAsync<User>(data.UserId);

            var entity = _mapper.Map<Part>(data);
            var result = await _context.CreateAsync(entity);
            return result > 0;
        }

        public async Task<bool> UpdateAsync(UpdatePartDto data)
        {
            var existing = await _context.GetByIdAsync<Part>(data.Id);

            if (data.Name        is not null) existing.Name        = data.Name;
            if (data.UnitCost.HasValue)       existing.UnitCost    = data.UnitCost.Value;
            if (data.Count.HasValue)          existing.Count       = data.Count.Value;
            if (data.Description is not null) existing.Description = data.Description;
            if (data.QRCode      is not null) existing.QRCode      = data.QRCode;
            if (data.Picture     is not null) existing.Picture     = data.Picture;

            var result = await _context.UpdateAsync(existing);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _context.GetByIdAsync<Part>(id);
            await _context.DeleteAsync(entity);
            return true;
        }

        public async Task<PartDetails?> GetByQRCodeAsync(string qrCode)
        {
            var part = await _context.Parts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.QRCode == qrCode);

            return part is null ? null : _mapper.Map<PartDetails>(part);
        }
    }
}
