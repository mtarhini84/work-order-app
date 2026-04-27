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
    public class AttachmentController : MainController
    {
        private readonly IAttachmentService _attachmentService;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AttachmentController(
            IAttachmentService attachmentService,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _attachmentService = attachmentService;
            _context           = context;
            _mapper            = mapper;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateAttachmentDto data)
        {
            bool result = await _attachmentService.CreateAsync(data);
            return IntoActionResult(result);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _context.GetByIdAsync<Attachment>(id);
            return IntoActionResult(_mapper.Map<AttachmentDetails>(result));
        }

        [HttpGet("request/{requestId}")]
        public async Task<IActionResult> GetByRequest(string requestId)
        {
            var result = await _context.GetEntitiesByFieldAsync<Attachment>("RequestId", requestId);
            return IntoActionResult(result.Select(_mapper.Map<AttachmentDetails>));
        }

        [HttpGet("work-order/{workOrderId}")]
        public async Task<IActionResult> GetByWorkOrder(string workOrderId)
        {
            var result = await _context.GetEntitiesByFieldAsync<Attachment>("WorkOrderId", workOrderId);
            return IntoActionResult(result.Select(_mapper.Map<AttachmentDetails>));
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Delete(string id)
        {
            bool result = await _attachmentService.DeleteAsync(id);
            return IntoActionResult(result);
        }

        [HttpPatch("activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(string id)
        {
            int result = await _context.ActivateAsync<Attachment>(id);
            return IntoActionResult(result);
        }

        [HttpPatch("deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(string id)
        {
            int result = await _context.DeactivateAsync<Attachment>(id);
            return IntoActionResult(result);
        }
    }
}
