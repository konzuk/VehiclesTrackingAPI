using VehicleTrackingAPI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Models
{
    public class Position : Resource
    {

        [Sortable(Default = true, DESC = true)]
        [SearchableDateTime]
        public DateTimeOffset CreatedAt { get; set; }


        public double Latitude { get; set; }
        public double Longitude { get; set; }



    }
}
