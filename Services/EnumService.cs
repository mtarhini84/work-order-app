using AutoMapper;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;
using WorkOrderApp.Services.Interfaces;

namespace WorkOrderApp.Services
{
    public class EnumService : IEnumService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public EnumService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper  = mapper;
        }

        public async Task<bool> CreateEnumAsync<T, Q>(Q data) where T : BaseEnum where Q : CreateEnumDto
        {
            var entity = _mapper.Map<T>(data);
            var result = await _context.CreateAsync(entity);
            return result > 0;
        }

        public async Task<bool> UpdateEnumAsync<T, Q>(Q data) where T : BaseEnum where Q : UpdateEnumDto
        {
            var existing = await _context.GetByIdAsync<T>(data.Id);
            existing.Name        = data.Name;
            existing.Description = data.Description;
            var result = await _context.UpdateAsync(existing);
            return result > 0;
        }

        public async Task<IEnumerable<Q>> GetAllAsync<T, Q>() where T : BaseEnum where Q : EnumDetails
            => await _context.GetAllDetailsAsync<T, Q>();

        public async Task<Q> GetByIdAsync<T, Q>(string id) where T : BaseEnum where Q : EnumDetails
        {
            var result = await _context.GetByIdAsync<T>(id);
            return _mapper.Map<Q>(result);
        }

        public async Task DeleteAsync<T>(string id) where T : BaseEnum
        {
            var entity = await _context.GetByIdAsync<T>(id);
            await _context.DeleteAsync(entity);
        }
    }
}
