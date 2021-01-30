using VehicleTrackingAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Services
{
    public interface IPlaceService
    {
        Task<Collection<Place>> GetPlaceForPositionAsync(Guid positionId);
    }

}
