using AutoMapper;
using AutoMapper.QueryableExtensions;
using VehicleTrackingAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace VehicleTrackingAPI.Services
{
    public class DefaultPositionService : IPositionService
    {
        private readonly VTApiDbContext _context;
        private readonly IConfigurationProvider _mappingConfiguration;
        private readonly UserManager<UserEntity> _userManager;

        public DefaultPositionService(
            VTApiDbContext context,
            IConfigurationProvider mappingConfiguration,
            UserManager<UserEntity> userManager)
        {
            _context = context;
            _mappingConfiguration = mappingConfiguration;
            _userManager = userManager;
        }


        public async Task<Guid> CreatePositionAsync(Guid userId, Guid vehicleId, PositionRegisterForm positionRegisterForm)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new InvalidOperationException("You must be logged in.");

            var vehicle = await _context.Vehicles
              .SingleOrDefaultAsync(r => r.Id == vehicleId && r.User.Id == userId);

            if (vehicle == null) throw new ArgumentException("Invalid vehicle ID.");

            var id = Guid.NewGuid();

            var newVehicle = _context.Positions.Add(new PositionEntity
            {
                Id = id,
                CreatedAt = DateTimeOffset.UtcNow,
                VehicleId = vehicleId,
                Location = new Point(positionRegisterForm.Long, positionRegisterForm.Lat) { SRID = 4326 }
        });

            var created = await _context.SaveChangesAsync();
            if (created < 1) throw new InvalidOperationException("Could not create Position.");

            return id;
        }

       

        public async Task<Position> GetPositionAsync(Guid PositionId)
        {
            var entity = await _context.Positions
                .SingleOrDefaultAsync(b => b.Id == PositionId);

            if (entity == null) return null;

            var mapper = _mappingConfiguration.CreateMapper();
            return mapper.Map<Position>(entity);
        }


        public async Task<Position> GetCurrentPositionAsync(Guid vehicleId)
        {
            var entity = await _context.Positions
                .OrderBy(b=>b.CreatedAt)
                .LastOrDefaultAsync(b => b.VehicleId == vehicleId);

            if (entity == null) return null;

            var mapper = _mappingConfiguration.CreateMapper();
            return mapper.Map<Position>(entity);
        }

        public async Task<PagedResults<Position>> GetPositionsForVehicleAsync(
            Guid vehicleId,
            PagingOptions pagingOptions,
            SortOptions<Position, PositionEntity> sortOptions,
            SearchOptions<Position, PositionEntity> searchOptions)
        {
            IQueryable<PositionEntity> query = _context.Positions
                //.AsNoTracking()
                .Where(b => b.VehicleId == vehicleId);
            query = searchOptions.Apply(query);
            query = sortOptions.Apply(query);


            var size = await query.CountAsync();

            var mapper = _mappingConfiguration.CreateMapper();

            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .Select(s=>mapper.Map<Position>(s))
                .ToArrayAsync();


            return new PagedResults<Position>
            {
                Items = items,
                TotalSize = size
            };
        }
    }
}
