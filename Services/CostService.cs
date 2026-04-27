using AutoMapper;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;
using WorkOrderApp.Services.Interfaces;

namespace WorkOrderApp.Services
{
    public class CostService : ICostService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CostService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper  = mapper;
        }

        public async Task<bool> CreateAsync(CreateCostDto data)
        {
            await _context.GetByIdAsync<WorkOrder>(data.WorkOrderId);
            await _context.GetByIdAsync<User>(data.UserId);

            var entity = _mapper.Map<Cost>(data);
            var result = await _context.CreateAsync(entity);
            return result > 0;
        }

        public async Task<bool> UpdateAsync(UpdateCostDto data)
        {
            var existing = await _context.GetByIdAsync<Cost>(data.Id);

            if (data.Name        is not null) existing.Name        = data.Name;
            if (data.Amount.HasValue)         existing.Amount      = data.Amount.Value;
            if (data.Description is not null) existing.Description = data.Description;
            if (data.Category    is not null) existing.Category    = data.Category;
            if (data.Picture     is not null) existing.Picture     = data.Picture;

            var result = await _context.UpdateAsync(existing);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _context.GetByIdAsync<Cost>(id);
            await _context.DeleteAsync(entity);
            return true;
        }
    }
}
