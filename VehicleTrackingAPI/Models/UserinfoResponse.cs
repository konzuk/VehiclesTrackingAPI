using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Models
{
    public class UserinfoResponse : Resource
    {
        [JsonProperty(PropertyName = OpenIddict.Abstractions.OpenIddictConstants.Claims.Subject)]
        public string Subject { get; set; }

        [JsonProperty(PropertyName = OpenIddict.Abstractions.OpenIddictConstants.Claims.GivenName)]
        public string GivenName { get; set; }

        [JsonProperty(PropertyName = OpenIddict.Abstractions.OpenIddictConstants.Claims.FamilyName)]
        public string FamilyName { get; set; }
    }
}
