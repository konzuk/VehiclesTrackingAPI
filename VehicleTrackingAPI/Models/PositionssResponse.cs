using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleTrackingAPI.Models;

namespace VehicleTrackingAPI.Models
{
    public class PositionsResponse : PagedCollection<Position>
    {

        public Form PositionQuery { get; set; }

    }
}
