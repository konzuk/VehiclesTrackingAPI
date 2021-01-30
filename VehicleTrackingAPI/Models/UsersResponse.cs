using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleTrackingAPI.Models;

namespace VehicleTrackingAPI.Models
{
    public class UsersResponse : PagedCollection<User>
    {

        public Form UserQuery { get; set; }
        public Form Register { get; set; }
        public Link Me { get; set; }
    }
}
