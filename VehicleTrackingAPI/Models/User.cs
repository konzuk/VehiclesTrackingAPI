using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleTrackingAPI.Infrastructure;

namespace VehicleTrackingAPI.Models
{
    public class User : Resource
    {
        public Link ListVehicles { get; set; }

        [Sortable]
        [SearchableString]
        public string Email { get; set; }
        [Sortable]
        [SearchableString]
        public string FirstName { get; set; }
        [Sortable]
        [SearchableString]
        public string LastName { get; set; }

        [Sortable(Default = true)]
        [SearchableDateTime]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
