using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;
using WorkOrderApp.Services.Interfaces;

namespace WorkOrderApp.Services
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LocationService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper  = mapper;
        }

        public async Task<bool> CreateAsync(CreateLocationDto data)
        {
            var entity = _mapper.Map<Location>(data);
            var result = await _context.CreateAsync(entity);
            return result > 0;
        }

        public async Task<bool> UpdateAsync(UpdateLocationDto data)
        {
            var existing = await _context.GetByIdAsync<Location>(data.Id);
            _mapper.Map(data, existing);
            var result = await _context.UpdateAsync(existing);
            return result > 0;
        }

        public async Task<bool> AssignUserAsync(AssignUserToLocationDto data)
        {
            // Validate both sides exist before touching the join table
            await _context.GetByIdAsync<User>(data.UserId);
            await _context.GetByIdAsync<Location>(data.LocationId);

            var already = await _context.UserLocations
                .AnyAsync(ul => ul.UserId == data.UserId && ul.LocationId == data.LocationId);

            if (already) return true; // idempotent

            _context.UserLocations.Add(new UserLocation
            {
                UserId     = data.UserId,
                LocationId = data.LocationId
            });

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveUserAsync(AssignUserToLocationDto data)
        {
            var entry = await _context.UserLocations
                .FirstOrDefaultAsync(ul => ul.UserId == data.UserId && ul.LocationId == data.LocationId);

            if (entry is null) return true; // idempotent

            _context.UserLocations.Remove(entry);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<UserDetails>> GetUsersAsync(string locationId)
        {
            var users = await _context.UserLocations
                .Where(ul => ul.LocationId == locationId)
                .Include(ul => ul.User)
                .Select(ul => ul.User)
                .AsNoTracking()
                .ToListAsync();

            return users.Select(_mapper.Map<UserDetails>);
        }

        public async Task<IEnumerable<LocationDetails>> GetUserLocationsAsync(string userId)
        {
            var locations = await _context.UserLocations
                .Where(ul => ul.UserId == userId)
                .Include(ul => ul.Location)
                .Select(ul => ul.Location)
                .AsNoTracking()
                .ToListAsync();

            return locations.Select(_mapper.Map<LocationDetails>);
        }
    }
}
