using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Entities;
using WorkOrderApp.Services.Interfaces;

namespace WorkOrderApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CostController : MainController
    {
        private readonly ICostService _costService;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CostController(ICostService costService, ApplicationDbContext context, IMapper mapper)
        {
            _costService = costService;
            _context     = context;
            _mapper      = mapper;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Operator,Executor")]
        public async Task<IActionResult> Create([FromBody] CreateCostDto data)
        {
            bool result = await _costService.CreateAsync(data);
            return IntoActionResult(result);
        }

        [HttpPost("update")]
        [Authorize(Roles = "Admin,Operator,Executor")]
        public async Task<IActionResult> Update([FromBody] UpdateCostDto data)
        {
            bool result = await _costService.UpdateAsync(data);
            return IntoActionResult(result);
        }

        [HttpGet("get/{id}")]
        [Authorize(Roles = "Admin,Operator,Executor")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _context.GetByIdAsync<Cost>(id);
            return IntoActionResult(_mapper.Map<CostDetails>(result));
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Delete(string id)
        {
            bool result = await _costService.DeleteAsync(id);
            return IntoActionResult(result);
        }

        [HttpPatch("activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(string id)
        {
            int result = await _context.ActivateAsync<Cost>(id);
            return IntoActionResult(result);
        }

        [HttpPatch("deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(string id)
        {
            int result = await _context.DeactivateAsync<Cost>(id);
            return IntoActionResult(result);
        }
    }
}
