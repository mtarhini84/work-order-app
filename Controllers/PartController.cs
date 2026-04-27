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
    public class PartController : MainController
    {
        private readonly IPartService _partService;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PartController(IPartService partService, ApplicationDbContext context, IMapper mapper)
        {
            _partService = partService;
            _context     = context;
            _mapper      = mapper;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Operator,Executor")]
        public async Task<IActionResult> Create([FromBody] CreatePartDto data)
        {
            bool result = await _partService.CreateAsync(data);
            return IntoActionResult(result);
        }

        [HttpPost("update")]
        [Authorize(Roles = "Admin,Operator,Executor")]
        public async Task<IActionResult> Update([FromBody] UpdatePartDto data)
        {
            bool result = await _partService.UpdateAsync(data);
            return IntoActionResult(result);
        }

        [HttpGet("get/{id}")]
        [Authorize(Roles = "Admin,Operator,Executor")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _context.GetByIdAsync<Part>(id);
            return IntoActionResult(_mapper.Map<PartDetails>(result));
        }

        [HttpGet("qr/{qrCode}")]
        [Authorize(Roles = "Admin,Operator,Executor")]
        public async Task<IActionResult> GetByQRCode(string qrCode)
        {
            var result = await _partService.GetByQRCodeAsync(qrCode);
            return IntoActionResult(result);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Delete(string id)
        {
            bool result = await _partService.DeleteAsync(id);
            return IntoActionResult(result);
        }

        [HttpPatch("activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(string id)
        {
            int result = await _context.ActivateAsync<Part>(id);
            return IntoActionResult(result);
        }

        [HttpPatch("deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(string id)
        {
            int result = await _context.DeactivateAsync<Part>(id);
            return IntoActionResult(result);
        }
    }
}
