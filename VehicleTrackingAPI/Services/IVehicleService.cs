using VehicleTrackingAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Services
{
    public interface IVehicleService
    {
        Task<Vehicle> GetVehicleAsync(Guid VehicleId);

        Task<Guid> CreateVehicleAsync(Guid userId, VehicleRegisterForm vehicleRegisterForm);

        //Task DeleteVehicleAsync(Guid VehicleId);

        Task<PagedResults<Vehicle>> GetVehiclesAsync(
            PagingOptions pagingOptions,
            SortOptions<Vehicle, VehicleEntity> sortOptions,
            SearchOptions<Vehicle, VehicleEntity> searchOptions);

        Task<Vehicle> GetVehicleForUserIdAsync(
            Guid VehicleId,
            Guid userId);

        Task<PagedResults<Vehicle>> GetVehiclesForUserIdAsync(
            Guid userId,
            PagingOptions pagingOptions,
            SortOptions<Vehicle, VehicleEntity> sortOptions,
            SearchOptions<Vehicle, VehicleEntity> searchOptions);
    }

}
