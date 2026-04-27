using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;

namespace WorkOrderApp.Services.Interfaces
{
    public interface IEnumService
    {
        Task<bool> CreateEnumAsync<T, Q>(Q data) where T : BaseEnum where Q : CreateEnumDto;
        Task<bool> UpdateEnumAsync<T, Q>(Q data) where T : BaseEnum where Q : UpdateEnumDto;
        Task<IEnumerable<Q>> GetAllAsync<T, Q>() where T : BaseEnum where Q : EnumDetails;
        Task<Q> GetByIdAsync<T, Q>(string id) where T : BaseEnum where Q : EnumDetails;
        Task DeleteAsync<T>(string id) where T : BaseEnum;
    }
}
