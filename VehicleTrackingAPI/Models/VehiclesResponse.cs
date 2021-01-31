using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleTrackingAPI.Models;

namespace VehicleTrackingAPI.Models
{
    public class VehiclesResponse : PagedCollection<Vehicle>
    {
        public Form VehicleQuery { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Form CreateVehicle { get; set; }

    }
}
