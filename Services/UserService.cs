using AutoMapper;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;
using WorkOrderApp.Exceptions;
using WorkOrderApp.Helpers.Auth;
using WorkOrderApp.Helpers.Utils;
using WorkOrderApp.Services.Interfaces;

namespace WorkOrderApp.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;

        public UserService(ApplicationDbContext context, IMapper mapper, IJwtService jwtService)
        {
            _context    = context;
            _mapper     = mapper;
            _jwtService = jwtService;
        }

        public async Task<bool> CreateAsync(CreateUserDto data)
        {
            var existing = await _context.GetEntityByFieldAsync<User>("Email", data.Email);
            if (existing != null)
                throw new BadRequestException("Email already in use");

            if (!PasswordUtils.ValidatePassword(data.Password))
                throw new BadRequestException("Password not strong enough");

            var entity = _mapper.Map<User>(data);
            entity.PasswordHash = PasswordUtils.HashPassword(data.Password);

            await _context.CreateAsync(entity);
            return true;
        }

        public async Task<bool> UpdateAsync(UpdateUserDto data)
        {
            var existing = await _context.GetByIdAsync<User>(data.Id);
            _mapper.Map(data, existing);
            var result = await _context.UpdateAsync(existing);
            return result > 0;
        }

        public async Task<AuthResult> LoginAsync(LoginModel data)
        {
            var user = await _context.GetEntityByFieldAsync<User>("Email", data.Identifier);

            if (user == null || !PasswordUtils.VerifyPassword(user.PasswordHash, data.Password))
                throw new BadRequestException("Wrong email or password");

            return new AuthResult
            {
                Token = _jwtService.GenerateToken(user.Id, user.Name, user.Email, user.Role),
                Data  = _mapper.Map<UserDetails>(user)
            };
        }

        public async Task<AuthResult> GetDetailsWithTokenAsync(string id)
        {
            var user = await _context.GetByIdAsync<User>(id);

            return new AuthResult
            {
                Token = _jwtService.GenerateToken(user.Id, user.Name, user.Email, user.Role),
                Data  = _mapper.Map<UserDetails>(user)
            };
        }

        public async Task<bool> UpdatePasswordAsync(UpdatePasswordModel data)
        {
            var existing = await _context.GetByIdAsync<User>(data.Id!);

            if (!PasswordUtils.VerifyPassword(existing.PasswordHash, data.OldPassword))
                throw new BadRequestException("Incorrect password");

            if (!PasswordUtils.ValidatePassword(data.NewPassword))
                throw new BadRequestException("New password not strong enough");

            existing.PasswordHash = PasswordUtils.HashPassword(data.NewPassword);
            var result = await _context.UpdateAsync(existing);
            return result > 0;
        }

        public async Task<bool> MakeAdminAsync(string id)
        {
            var existing = await _context.GetByIdAsync<User>(id);
            existing.Role = "Admin";
            var result = await _context.UpdateAsync(existing);
            return result > 0;
        }
    }
}
