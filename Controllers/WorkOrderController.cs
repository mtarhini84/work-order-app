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
    public class WorkOrderController : MainController
    {
        private readonly IWorkOrderService _workOrderService;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public WorkOrderController(
            IWorkOrderService workOrderService,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _workOrderService = workOrderService;
            _context          = context;
            _mapper           = mapper;
        }

        // ── Write ─────────────────────────────────────────────────────────────

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Create([FromBody] CreateWorkOrderDto data)
        {
            bool result = await _workOrderService.CreateAsync(data);
            return IntoActionResult(result);
        }

        [HttpPost("update")]
        [Authorize(Roles = "Admin,Operator,Executor")]
        public async Task<IActionResult> Update([FromBody] UpdateWorkOrderDto data)
        {
            bool result = await _workOrderService.UpdateAsync(data, GetUserId());
            return IntoActionResult(result);
        }

        [HttpPost("assign")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Assign([FromBody] AssignWorkOrderDto data)
        {
            bool result = await _workOrderService.AssignAsync(data, GetUserId());
            return IntoActionResult(result);
        }

        [HttpPatch("activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(string id)
        {
            int result = await _context.ActivateAsync<WorkOrder>(id);
            return IntoActionResult(result);
        }

        [HttpPatch("deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(string id)
        {
            int result = await _context.DeactivateAsync<WorkOrder>(id);
            return IntoActionResult(result);
        }

        // ── Read ──────────────────────────────────────────────────────────────

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _context.GetByIdAsync<WorkOrder>(
                id,
                wo => wo.Location,
                wo => wo.Customer,
                wo => wo.AssignedTo,
                wo => wo.AssignedBy);

            return IntoActionResult(_mapper.Map<WorkOrderDetails>(result));
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _context.GetAllDetailsAsync<WorkOrder, WorkOrderDetails>(
                wo => wo.Location,
                wo => wo.Customer,
                wo => wo.AssignedTo);

            return IntoActionResult(result);
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> GetActive()
        {
            var result = await _context.GetAllActiveDetailsAsync<WorkOrder, WorkOrderDetails>(
                wo => wo.Location,
                wo => wo.Customer,
                wo => wo.AssignedTo);

            return IntoActionResult(result);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Admin,Operator,Executor")]
        public async Task<IActionResult> GetMine()
        {
            var result = await _context.GetEntitiesByFieldAsync<WorkOrder>(
                "AssignedToId", GetUserId(),
                wo => wo.Location,
                wo => wo.Customer);

            return IntoActionResult(result.Select(_mapper.Map<WorkOrderDetails>));
        }

        [HttpGet("location/{locationId}")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> GetByLocation(string locationId)
        {
            var result = await _context.GetEntitiesByFieldAsync<WorkOrder>(
                "LocationId", locationId,
                wo => wo.Customer,
                wo => wo.AssignedTo);

            return IntoActionResult(result.Select(_mapper.Map<WorkOrderDetails>));
        }

        [HttpGet("assigned/{userId}")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> GetByAssignee(string userId)
        {
            var result = await _context.GetEntitiesByFieldAsync<WorkOrder>(
                "AssignedToId", userId,
                wo => wo.Location,
                wo => wo.Customer);

            return IntoActionResult(result.Select(_mapper.Map<WorkOrderDetails>));
        }

        // ── Sub-resources ─────────────────────────────────────────────────────

        [HttpGet("{id}/logs")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> GetLogs(string id)
        {
            var result = await _context.GetEntitiesByFieldAsync<WorkOrderLog>("WorkOrderId", id);
            return IntoActionResult(result.Select(_mapper.Map<WorkOrderLogDetails>));
        }

        [HttpGet("{id}/costs")]
        [Authorize(Roles = "Admin,Operator,Executor")]
        public async Task<IActionResult> GetCosts(string id)
        {
            var result = await _context.GetEntitiesByFieldAsync<Cost>("WorkOrderId", id);
            return IntoActionResult(result.Select(_mapper.Map<CostDetails>));
        }

        [HttpGet("{id}/parts")]
        [Authorize(Roles = "Admin,Operator,Executor")]
        public async Task<IActionResult> GetParts(string id)
        {
            var result = await _context.GetEntitiesByFieldAsync<Part>("WorkOrderId", id);
            return IntoActionResult(result.Select(_mapper.Map<PartDetails>));
        }

        [HttpGet("{id}/attachments")]
        public async Task<IActionResult> GetAttachments(string id)
        {
            var result = await _context.GetEntitiesByFieldAsync<Attachment>("WorkOrderId", id);
            return IntoActionResult(result.Select(_mapper.Map<AttachmentDetails>));
        }
    }
}
