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
        private readonly IVehicleService _vehicleService;
        private readonly IUserService _userService;
        private readonly IPositionService _positionService;
        private readonly IAuthorizationService _authzService;
        private readonly PagingOptions _defaultPagingOptions;


       
        public VehiclesController(
            IVehicleService vehicleService,
            IUserService userService,
            IPositionService positionService,
            IAuthorizationService authzService,
            IOptions<PagingOptions> defaultPagingOptionsAccessor)
        {
            _vehicleService = vehicleService;
            _positionService = positionService;
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


            var VehicleId = await _vehicleService.CreateVehicleAsync(
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
                    Vehicles = await _vehicleService.GetVehiclesAsync(
                        pagingOptions, sortOptions, searchOptions);
                }
                else
                {
                    var userId = await _userService.GetUserIdAsync(User);
                    if (userId != null)
                    {
                        Vehicles = await _vehicleService.GetVehiclesForUserIdAsync(
                            userId.Value, pagingOptions, sortOptions, searchOptions);
                    }
                }
            }

            var collectionLink = Link.To(nameof(GetVisibleVehicles));
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
        public async Task<ActionResult<Vehicle>> GetVehicleById(Guid vehicleId)
        {
            var userId = await _userService.GetUserIdAsync(User);
            if (userId == null) return Unauthorized();
            
            var canViewAllVehicles = await _authzService.AuthorizeAsync(
                User, "ViewAllVehiclesPolicy");


            Vehicle Vehicle;

            if (canViewAllVehicles.Succeeded)
            {
                Vehicle = await _vehicleService.GetVehicleAsync(vehicleId);
            }
            else
            {
                Vehicle = await _vehicleService.GetVehicleForUserIdAsync(
                    vehicleId, userId.Value);
            }

            if (Vehicle == null) return NotFound();

            return Vehicle;
        }


        // GET /vehicles/{vehicleId}/currentPosition
        [Authorize]
        [HttpGet("{VehicleId}/currentPosition", Name = nameof(GetCurrPositionByVehicleId))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Position>> GetCurrPositionByVehicleId(Guid vehicleId)
        {
            var userId = await _userService.GetUserIdAsync(User);
            if (userId == null) return Unauthorized();

            var vehicle = await _vehicleService.GetVehicleAsync(vehicleId);
            if (vehicle == null) return NotFound();

            var canViewAllVehicles = await _authzService.AuthorizeAsync(
                User, "ViewAllVehiclesPositionPolicy");


            Position position;

            if (canViewAllVehicles.Succeeded)
            {
                position = await _positionService.GetCurrentPositionAsync(vehicleId);
            }
            else
            {
                return Unauthorized();
            }

            if (position == null) return NotFound();

            return position;
        }


        // POST /vehicles/{vehicleId}/positions
        [Authorize]
        [HttpPost("{vehicleId}/positions", Name = nameof(CreatePositionForVehicle))]
        [ProducesResponseType(401)]
        [ProducesResponseType(201)]
        public async Task<ActionResult> CreatePositionForVehicle(
            Guid vehicleId, [FromBody] PositionRegisterForm form)
        {
            var userId = await _userService.GetUserIdAsync(User);
            if (userId == null) return Unauthorized();


            var positionId = await _positionService.CreatePositionAsync(
                userId.Value, vehicleId, form);

            return Created("",null);
        }


        // GET /vehicles/{vehicleId}/listPositions
        [HttpGet("{vehicleId}/listPositions", Name = nameof(GetPositionsForVehicle))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetPositionsForVehicle(
            Guid vehicleId,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Position, PositionEntity> sortOptions,
            [FromQuery] SearchOptions<Position, PositionEntity> searchOptions)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var userId = await _userService.GetUserIdAsync(User);
            if (userId == null) return Unauthorized();

            var vehicle = await _vehicleService.GetVehicleAsync(vehicleId);
            if (vehicle == null) return NotFound();

            var canViewAllVehicles = await _authzService.AuthorizeAsync(
                User, "ViewAllVehiclesPositionsPolicy");
            if(!canViewAllVehicles.Succeeded) return Unauthorized();


            var positions = await _positionService.GetPositionsForVehicleAsync(
                vehicleId,
                pagingOptions,
                sortOptions,
                searchOptions);

            var collectionLink = Link.To(
                nameof(GetPositionsForVehicle), new { vehicleId });

            var collection = PagedCollection<Position>.Create<PositionsResponse>(
                collectionLink,
                positions.Items.ToArray(),
                positions.TotalSize,
                pagingOptions);

            collection.PositionQuery = FormMetadata.FromResource<Position>(
               Link.ToForm(
                   nameof(GetPositionsForVehicle),
                   null,
                   Link.GetMethod, null,
                   Form.QueryRelation));



            return Ok(collection);
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

        //    var Vehicle = await _vehicleService.GetVehicleForUserIdAsync(
        //        VehicleId, userId.Value);
        //    if (Vehicle != null)
        //    {
        //        await _vehicleService.DeleteVehicleAsync(VehicleId);
        //        return NoContent();
        //    }

        //    var canViewAllVehicles = await _authzService.AuthorizeAsync(
        //        User, "ViewAllVehiclesPolicy");
        //    if (!canViewAllVehicles.Succeeded)
        //    {
        //        return NotFound();
        //    }

        //    Vehicle = await _vehicleService.GetVehicleAsync(VehicleId);
        //    if (Vehicle == null) return NotFound();

        //    await _vehicleService.DeleteVehicleAsync(VehicleId);
        //    return NoContent();
        //}
    }

}
