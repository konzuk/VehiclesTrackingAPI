using VehicleTrackingAPI.Models;
using VehicleTrackingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleTrackingAPI.Infrastructure;

namespace VehicleTrackingAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _VehicleService;
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authzService;
        private readonly PagingOptions _defaultPagingOptions;


       
        public VehiclesController(
            IVehicleService VehicleService,
            IUserService userService,
            IAuthorizationService authzService,
            IOptions<PagingOptions> defaultPagingOptionsAccessor)
        {
            _VehicleService = VehicleService;
            _userService = userService;
            _authzService = authzService;
            _defaultPagingOptions = defaultPagingOptionsAccessor.Value;
        }


        [Authorize]
        [HttpPost(Name = nameof(CreateVehicle))]
        [ProducesResponseType(401)]
        [ProducesResponseType(201)]
        public async Task<ActionResult> CreateVehicle([FromBody] VehicleRegisterForm form)
        {
            var userId = await _userService.GetUserIdAsync(User);
            if (userId == null) return Unauthorized();


            var VehicleId = await _VehicleService.CreateVehicleAsync(
                userId.Value, form);

            return Created(
                Url.Link(nameof(VehiclesController.GetVehicleById),
                new { VehicleId }),
                null);
        }


        [Authorize]
        [HttpGet(Name = nameof(GetVisibleVehicles))]
        [ProducesResponseType(401)]
        [ProducesResponseType(200)]
        public async Task<PagedCollection<Vehicle>> GetVisibleVehicles(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Vehicle, VehicleEntity> sortOptions,
            [FromQuery] SearchOptions<Vehicle, VehicleEntity> searchOptions)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var Vehicles = new PagedResults<Vehicle>();

            if (User.Identity.IsAuthenticated)
            {
                var userCanSeeAllVehicles = await _authzService.AuthorizeAsync(
                    User, "ViewAllVehiclesPolicy");
                if (userCanSeeAllVehicles.Succeeded)
                {
                    Vehicles = await _VehicleService.GetVehiclesAsync(
                        pagingOptions, sortOptions, searchOptions);
                }
                else
                {
                    var userId = await _userService.GetUserIdAsync(User);
                    if (userId != null)
                    {
                        Vehicles = await _VehicleService.GetVehiclesForUserIdAsync(
                            userId.Value, pagingOptions, sortOptions, searchOptions);
                    }
                }
            }

            var collectionLink = Link.ToCollection(nameof(GetVisibleVehicles));
            var collection = PagedCollection<Vehicle>.Create<VehiclesResponse>(
                collectionLink,
                Vehicles.Items.ToArray(),
                Vehicles.TotalSize,
                pagingOptions);

            collection.VehicleQuery = FormMetadata.FromResource<Vehicle>(
                Link.ToForm(
                    nameof(GetVisibleVehicles),
                    null,
                    Link.GetMethod, null,
                    Form.QueryRelation));


            collection.CreateVehicle = FormMetadata.FromModel(
                new VehicleRegisterForm(),
                Link.ToForm(nameof(CreateVehicle), relations: Form.CreateRelation));

            return collection;
        }

        [Authorize]
        [HttpGet("{VehicleId}", Name = nameof(GetVehicleById))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Vehicle>> GetVehicleById(Guid VehicleId)
        {
            var userId = await _userService.GetUserIdAsync(User);
            if (userId == null) return NotFound();

            Vehicle Vehicle = null;

            var canViewAllVehicles = await _authzService.AuthorizeAsync(
                User, "ViewAllVehiclesPolicy");

            if (canViewAllVehicles.Succeeded)
            {
                Vehicle = await _VehicleService.GetVehicleAsync(VehicleId);
            }
            else
            {
                Vehicle = await _VehicleService.GetVehicleForUserIdAsync(
                    VehicleId, userId.Value);
            }

            if (Vehicle == null) return NotFound();

            return Vehicle;
        }

        // DELETE /Vehicles/{VehicleId}
        //[Authorize]
        //[HttpDelete("{VehicleId}", Name = nameof(DeleteVehicleById))]
        //[ProducesResponseType(204)]
        //[ProducesResponseType(401)]
        //[ProducesResponseType(404)]
        //public async Task<IActionResult> DeleteVehicleById(Guid VehicleId)
        //{
        //    var userId = await _userService.GetUserIdAsync(User);
        //    if (userId == null) return NotFound();

        //    var Vehicle = await _VehicleService.GetVehicleForUserIdAsync(
        //        VehicleId, userId.Value);
        //    if (Vehicle != null)
        //    {
        //        await _VehicleService.DeleteVehicleAsync(VehicleId);
        //        return NoContent();
        //    }

        //    var canViewAllVehicles = await _authzService.AuthorizeAsync(
        //        User, "ViewAllVehiclesPolicy");
        //    if (!canViewAllVehicles.Succeeded)
        //    {
        //        return NotFound();
        //    }

        //    Vehicle = await _VehicleService.GetVehicleAsync(VehicleId);
        //    if (Vehicle == null) return NotFound();

        //    await _VehicleService.DeleteVehicleAsync(VehicleId);
        //    return NoContent();
        //}
    }

}
