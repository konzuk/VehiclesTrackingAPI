using VehicleTrackingAPI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Models
{
    public class Vehicle : Resource
    {

        public Link User { get; set; }



       

        [Sortable]
        [SearchableString]
        public string Name { get; set; }



        [Sortable]
        [SearchableString]
        public string UserName { get; set; }

        public string Description { get; set; }


        [Sortable(Default = true)]
        [SearchableDateTime]
        public DateTimeOffset CreatedAt { get; set; }

        [Sortable]
        [SearchableDateTime]
        public DateTimeOffset ModifiedAt { get; set; }

    }
}
