using VehicleTrackingAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OpenIddict.Server.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using System;
using VehicleTrackingAPI.Services;

namespace VehicleTrackingAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {

        private readonly IAuthorizationService _authzService;
        private readonly IUserService _userService;
        private readonly IPositionService _positionService;
        private readonly IPlaceService _placeService;
        public PositionsController(
            IAuthorizationService authorizationService,
            IUserService userService,
            IPositionService positionService,
            IPlaceService placeService
            )
        {
            _authzService = authorizationService;
            _userService = userService;
            _positionService = positionService;
            _placeService = placeService;
        }

        [Authorize]
        [HttpGet("{positionId}/place", Name = nameof(GetPlaceForPosition))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        //ResponseCache Server catch for 1 Day to reduct number Of API call. 
        [ResponseCache(CacheProfileName = "Static")]
        public async Task<ActionResult<Collection<Place>>> GetPlaceForPosition(Guid positionId)
        {

            var Places = new Collection<Place>();

            if (User.Identity.IsAuthenticated)
            {
                var canSeeEveryone = await _authzService.AuthorizeAsync(
                    User, "ViewAllPositionsPlacesPolicy");

                var position = await _positionService.GetPositionAsync(positionId);
                if (position == null) return NotFound();

                if (canSeeEveryone.Succeeded)
                {
                    Places = await _placeService.GetPlaceForPositionAsync(positionId);
                }
                else
                {
                    return Unauthorized();
                }
            }

            return Places;
        }

    }
}
