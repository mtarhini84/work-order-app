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
    public class RequestController : MainController
    {
        private readonly IRequestService _requestService;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RequestController(
            IRequestService requestService,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _requestService = requestService;
            _context        = context;
            _mapper         = mapper;
        }

        // ── Write ─────────────────────────────────────────────────────────────

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Operator,Customer")]
        public async Task<IActionResult> Create([FromBody] CreateRequestDto data)
        {
            bool result = await _requestService.CreateAsync(data, GetUserId());
            return IntoActionResult(result);
        }

        [HttpPost("update")]
        [Authorize(Roles = "Admin,Operator,Customer")]
        public async Task<IActionResult> Update([FromBody] UpdateRequestDto data)
        {
            bool result = await _requestService.UpdateAsync(data, GetUserId());
            return IntoActionResult(result);
        }

        [HttpPost("approve")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Approve([FromBody] ApproveRequestDto data)
        {
            bool result = await _requestService.ApproveAsync(data, GetUserId());
            return IntoActionResult(result);
        }

        [HttpPost("decline")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Decline([FromBody] DeclineRequestDto data)
        {
            bool result = await _requestService.DeclineAsync(data, GetUserId());
            return IntoActionResult(result);
        }

        [HttpPatch("done")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> MarkDone(string id)
        {
            bool result = await _requestService.MarkDoneAsync(id, GetUserId());
            return IntoActionResult(result);
        }

        [HttpPatch("activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(string id)
        {
            int result = await _context.ActivateAsync<Request>(id);
            return IntoActionResult(result);
        }

        [HttpPatch("deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(string id)
        {
            int result = await _context.DeactivateAsync<Request>(id);
            return IntoActionResult(result);
        }

        // ── Read ──────────────────────────────────────────────────────────────

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _context.GetByIdAsync<Request>(
                id,
                r => r.Location,
                r => r.RequestedBy,
                r => r.Attachments);

            return IntoActionResult(_mapper.Map<RequestDetails>(result));
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _context.GetAllDetailsAsync<Request, RequestDetails>(
                r => r.Location,
                r => r.RequestedBy);

            return IntoActionResult(result);
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> GetActive()
        {
            var result = await _context.GetAllActiveDetailsAsync<Request, RequestDetails>(
                r => r.Location,
                r => r.RequestedBy);

            return IntoActionResult(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMine()
        {
            var result = await _context.GetEntitiesByFieldAsync<Request>(
                "RequestedById", GetUserId(),
                r => r.Location);

            return IntoActionResult(result.Select(_mapper.Map<RequestDetails>));
        }

        [HttpGet("location/{locationId}")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> GetByLocation(string locationId)
        {
            var result = await _context.GetEntitiesByFieldAsync<Request>(
                "LocationId", locationId,
                r => r.RequestedBy);

            return IntoActionResult(result.Select(_mapper.Map<RequestDetails>));
        }

        [HttpGet("{id}/logs")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> GetLogs(string id)
        {
            var result = await _context.GetEntitiesByFieldAsync<RequestLog>(
                "RequestId", id);

            return IntoActionResult(result.Select(_mapper.Map<RequestLogDetails>));
        }
    }
}
