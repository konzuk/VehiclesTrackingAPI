using VehicleTrackingAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Services
{
    public interface IPositionService
    {

        Task<Guid> CreatePositionAsync(Guid userId, Guid vehicleId, PositionRegisterForm positionRegisterForm);
        Task<Position> GetPositionAsync(Guid PositionId);

        Task<Position> GetCurrentPositionAsync(Guid vehicleId);

        Task<PagedResults<Position>> GetPositionsForVehicleAsync(
            Guid vehicleId,
            PagingOptions pagingOptions,
            SortOptions<Position, PositionEntity> sortOptions,
            SearchOptions<Position, PositionEntity> searchOptions);
    }

}
