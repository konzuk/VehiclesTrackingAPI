using VehicleTrackingAPI.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Models
{
    public class RootResponse : Resource, IEtaggable
    {
        public Link UserInfo { get; set; }
        public Link Users { get; set; }
        public Link Vehicles { get; set; }

        public Form Token { get; set; }



        public string GetEtag()
        {
            var serialized = JsonConvert.SerializeObject(this);
            return Md5Hash.ForString(serialized);
        }
    }
}
