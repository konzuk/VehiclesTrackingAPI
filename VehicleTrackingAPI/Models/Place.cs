using VehicleTrackingAPI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Models
{
    public class Place : Resource 
    {

        [Sortable(Default = true, DESC = true)]
        [SearchableDateTime]
        public DateTimeOffset CreatedAt { get; set; }
        public string place_id { get; set; }
        public string types { get; set; }
        public string formatted_address { get; set; }

    }
}
