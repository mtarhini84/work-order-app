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
    public class LocationController : MainController
    {
        private readonly ILocationService _locationService;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LocationController(
            ILocationService locationService,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _locationService = locationService;
            _context         = context;
            _mapper          = mapper;
        }

        // ── CRUD ──────────────────────────────────────────────────────────────

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Create([FromBody] CreateLocationDto data)
        {
            bool result = await _locationService.CreateAsync(data);
            return IntoActionResult(result);
        }

        [HttpPost("update")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Update([FromBody] UpdateLocationDto data)
        {
            bool result = await _locationService.UpdateAsync(data);
            return IntoActionResult(result);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _context.GetByIdAsync<Location>(id);
            return IntoActionResult(_mapper.Map<LocationDetails>(result));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _context.GetAllDetailsAsync<Location, LocationDetails>();
            return IntoActionResult(result);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var result = await _context.GetAllActiveDetailsAsync<Location, LocationDetails>();
            return IntoActionResult(result);
        }

        [HttpPatch("activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(string id)
        {
            int result = await _context.ActivateAsync<Location>(id);
            return IntoActionResult(result);
        }

        [HttpPatch("deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(string id)
        {
            int result = await _context.DeactivateAsync<Location>(id);
            return IntoActionResult(result);
        }

        // ── User assignment ───────────────────────────────────────────────────

        [HttpPost("assign-user")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> AssignUser([FromBody] AssignUserToLocationDto data)
        {
            bool result = await _locationService.AssignUserAsync(data);
            return IntoActionResult(result);
        }

        [HttpPost("remove-user")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> RemoveUser([FromBody] AssignUserToLocationDto data)
        {
            bool result = await _locationService.RemoveUserAsync(data);
            return IntoActionResult(result);
        }

        [HttpGet("{id}/users")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> GetUsers(string id)
        {
            var result = await _locationService.GetUsersAsync(id);
            return IntoActionResult(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserLocations(string userId)
        {
            var result = await _locationService.GetUserLocationsAsync(userId);
            return IntoActionResult(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyLocations()
        {
            var result = await _locationService.GetUserLocationsAsync(GetUserId());
            return IntoActionResult(result);
        }
    }
}
