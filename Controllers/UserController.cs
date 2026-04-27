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
    public class UserController : MainController
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, ApplicationDbContext context, IMapper mapper)
        {
            _userService = userService;
            _context     = context;
            _mapper      = mapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel data)
        {
            var result = await _userService.LoginAsync(data);
            return IntoActionResult(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto user)
        {
            bool result = await _userService.CreateAsync(user);
            return IntoActionResult(result);
        }

        [HttpPost("update")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Update([FromBody] UpdateUserDto user)
        {
            user.Id = GetUserId();
            bool result = await _userService.UpdateAsync(user);
            return IntoActionResult(result);
        }

        [HttpPost("update-password")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordModel data)
        {
            data.Id = GetUserId();
            bool result = await _userService.UpdatePasswordAsync(data);
            return IntoActionResult(result);
        }

        [HttpGet("details")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetDetails()
        {
            var result = await _userService.GetDetailsWithTokenAsync(GetUserId());
            return IntoActionResult(result);
        }

        [HttpGet("get/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _context.GetByIdAsync<User>(id);
            return IntoActionResult(_mapper.Map<UserDetails>(result));
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _context.GetAllDetailsAsync<User, UserDetails>();
            return IntoActionResult(result);
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetActive()
        {
            var result = await _context.GetAllActiveDetailsAsync<User, UserDetails>();
            return IntoActionResult(result);
        }

        [HttpPatch("make-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MakeAdmin(string id)
        {
            bool result = await _userService.MakeAdminAsync(id);
            return IntoActionResult(result);
        }

        [HttpPatch("activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(string id)
        {
            int result = await _context.ActivateAsync<User>(id);
            return IntoActionResult(result);
        }

        [HttpPatch("deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(string id)
        {
            int result = await _context.DeactivateAsync<User>(id);
            return IntoActionResult(result);
        }
    }
}
