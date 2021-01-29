using AutoMapper;
using AutoMapper.QueryableExtensions;
using VehicleTrackingAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Services
{
    public class DefaultVehicleService : IVehicleService
    {
        private readonly VTApiDbContext _context;
        private readonly IConfigurationProvider _mappingConfiguration;
        private readonly UserManager<UserEntity> _userManager;

        public DefaultVehicleService(
            VTApiDbContext context,
            IConfigurationProvider mappingConfiguration,
            UserManager<UserEntity> userManager)
        {
            _context = context;
            _mappingConfiguration = mappingConfiguration;
            _userManager = userManager;
        }

        
        public async Task<Guid> CreateVehicleAsync(Guid userId, VehicleRegisterForm vehicleRegisterForm)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new InvalidOperationException("You must be logged in.");

            var id = Guid.NewGuid();

            var newVehicle = _context.Vehicles.Add(new VehicleEntity
            {
                Id = id,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow,
                User = user,
                Name = vehicleRegisterForm.Name,
                Description = vehicleRegisterForm.Description
            });

            var created = await _context.SaveChangesAsync();
            if (created < 1) throw new InvalidOperationException("Could not create Vehicle.");

            return id;
        }

        //public async Task DeleteVehicleAsync(Guid VehicleId)
        //{
        //    var Vehicle = await _context.Vehicles
        //        .SingleOrDefaultAsync(b => b.Id == VehicleId);
        //    if (Vehicle == null) return;

        //    _context.Vehicles.Remove(Vehicle);
        //    await _context.SaveChangesAsync();
        //}

        public async Task<Vehicle> GetVehicleAsync(Guid VehicleId)
        {
            var entity = await _context.Vehicles
                .Include(b => b.User)
                .SingleOrDefaultAsync(b => b.Id == VehicleId);

            if (entity == null) return null;

            var mapper = _mappingConfiguration.CreateMapper();
            return mapper.Map<Vehicle>(entity);
        }

        public async Task<Vehicle> GetVehicleForUserIdAsync(Guid VehicleId, Guid userId)
        {
            var entity = await _context.Vehicles
                .Include(b => b.User)
                .SingleOrDefaultAsync(b => b.Id == VehicleId && b.User.Id == userId);

            if (entity == null) return null;

            var mapper = _mappingConfiguration.CreateMapper();
            return mapper.Map<Vehicle>(entity);
        }

        public async Task<PagedResults<Vehicle>> GetVehiclesAsync(
            PagingOptions pagingOptions,
            SortOptions<Vehicle, VehicleEntity> sortOptions,
            SearchOptions<Vehicle, VehicleEntity> searchOptions)
        {
            IQueryable<VehicleEntity> query = _context.Vehicles
                .Include(b => b.User);
            query = searchOptions.Apply(query);
            query = sortOptions.Apply(query);

            var size = await query.CountAsync();

            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<Vehicle>(_mappingConfiguration)
                .ToArrayAsync();

            return new PagedResults<Vehicle>
            {
                Items = items,
                TotalSize = size
            };
        }

        public async Task<PagedResults<Vehicle>> GetVehiclesForUserIdAsync(
            Guid userId,
            PagingOptions pagingOptions,
            SortOptions<Vehicle, VehicleEntity> sortOptions,
            SearchOptions<Vehicle, VehicleEntity> searchOptions)
        {
            IQueryable<VehicleEntity> query = _context.Vehicles
                .Include(b => b.User)
                .Where(b => b.User.Id == userId);
            query = searchOptions.Apply(query);
            query = sortOptions.Apply(query);

            var size = await query.CountAsync();

            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<Vehicle>(_mappingConfiguration)
                .ToArrayAsync();

            return new PagedResults<Vehicle>
            {
                Items = items,
                TotalSize = size
            };
        }
    }
}
