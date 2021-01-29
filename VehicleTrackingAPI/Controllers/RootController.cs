using VehicleTrackingAPI.Controllers;
using VehicleTrackingAPI.Infrastructure;
using VehicleTrackingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Controllers
{
    [Route("/")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet(Name = nameof(GetRoot))]
        [ProducesResponseType(200)]
        [ProducesResponseType(304)]
        [ResponseCache(CacheProfileName = "Static")]
        [Etag]
        public IActionResult GetRoot()
        {
            var response = new RootResponse
            {
                Self = Link.To(nameof(GetRoot)),
                Users = Link.ToCollection(nameof(UsersController.GetVisibleUsers)),
                UserInfo = Link.To(nameof(UserinfoController.Userinfo)),
                Vehicles = Link.To(nameof(VehiclesController.GetVisibleVehicles)),
                Token = FormMetadata.FromModel(
                    new PasswordGrantForm(),
                    Link.ToForm(nameof(TokenController.TokenExchange),
                                null, mediaType: Form.XWwwMediaType, relations: Form.Relation))
            };

            if (!Request.GetEtagHandler().NoneMatch(response))
            {
                return StatusCode(304, response);
            }

            return Ok(response);
        }
    }
}
