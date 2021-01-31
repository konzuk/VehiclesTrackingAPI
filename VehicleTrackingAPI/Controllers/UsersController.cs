using VehicleTrackingAPI.Infrastructure;
using VehicleTrackingAPI.Models;
using VehicleTrackingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace VehicleTrackingAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IVehicleService _vehicleService;
        private readonly PagingOptions _defaultPagingOptions;
        private readonly IAuthorizationService _authzService;




        public UsersController(
            IUserService userService,
            IVehicleService vehicleService,
            IOptions<PagingOptions> defaultPagingOptions,
            IAuthorizationService authorizationService)
        {
            _userService = userService;
            _vehicleService = vehicleService;
            _defaultPagingOptions = defaultPagingOptions.Value;
            _authzService = authorizationService;
        }

        [Authorize]
        [ProducesResponseType(401)]
        [HttpGet(Name = nameof(GetVisibleUsers))]
        public async Task<ActionResult<PagedCollection<User>>> GetVisibleUsers(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<User, UserEntity> sortOptions,
            [FromQuery] SearchOptions<User, UserEntity> searchOptions)
        {

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var userCheck = await _userService.GetUserAsync(User);
            if (userCheck == null)
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = Errors.InvalidToken,
                    ErrorDescription = "The user does not exist."
                });
            }

            var users = new PagedResults<User>
            {
                Items = Enumerable.Empty<User>()
            };

            if (User.Identity.IsAuthenticated)
            {
                var canSeeEveryone = await _authzService.AuthorizeAsync(
                    User, "ViewAllUsersPolicy");
                if (canSeeEveryone.Succeeded)
                {
                    users = await _userService.GetUsersAsync(
                        pagingOptions, sortOptions, searchOptions);
                }
                else
                {
                    var myself = await _userService.GetUserAsync(User);
                    users.Items = new[] { myself };
                    users.TotalSize = 1;
                }
            }

            var collection = PagedCollection<User>.Create<UsersResponse>(
                Link.To(nameof(GetVisibleUsers)),
                users.Items.ToArray(),
                users.TotalSize,
                pagingOptions);

            collection.UserQuery = FormMetadata.FromResource<User>(
                Link.ToForm(
                    nameof(GetVisibleUsers),
                    null,
                    Link.GetMethod, null,
                    Form.QueryRelation));

            collection.Me = Link.To(nameof(UserinfoController.Userinfo));
            collection.Register = FormMetadata.FromModel(
                new RegisterForm(),
                Link.ToForm(nameof(RegisterUser), relations: Form.CreateRelation));

            return collection;
        }

        [Authorize]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [HttpGet("{userId}", Name = nameof(GetUserById))]
        public async Task<ActionResult<User>> GetUserById(Guid userId)
        {
            var userCheck = await _userService.GetUserAsync(User);
            if (userCheck == null)
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = Errors.InvalidToken,
                    ErrorDescription = "The user does not exist."
                });
            }
            var currentUserId = await _userService.GetUserIdAsync(User);
            if (currentUserId == null) return NotFound();

            if (currentUserId == userId)
            {
                var myself = await _userService.GetUserAsync(User);
                return myself;
            }

            var canSeeEveryone = await _authzService.AuthorizeAsync(
                User, "ViewAllUsersPolicy");
            if (!canSeeEveryone.Succeeded) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            return user;
        }

        // POST /users
        [HttpPost(Name = nameof(RegisterUser))]
        [ProducesResponseType(400)]
        [ProducesResponseType(201)]
        public async Task<IActionResult> RegisterUser(
            [FromBody] RegisterForm form)
        {
            var (succeeded, message) = await _userService.CreateUserAsync(form);
            if (succeeded) return Created(
                Url.Link(nameof(UserinfoController.Userinfo), null),
                null);

            return BadRequest(new ApiError
            {
                Message = "Registration failed.",
                Detail = message
            });
        }







        // GET /vehicles/{userId}/listVehicles
        // Assume that is route only for Admin role
        [Authorize]
        [HttpGet("{userId}/listVehicles", Name = nameof(GetVehiclesForUser))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetVehiclesForUser(
            Guid userId,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Vehicle, VehicleEntity> sortOptions,
            [FromQuery] SearchOptions<Vehicle, VehicleEntity> searchOptions)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var userCheck = await _userService.GetUserAsync(User);
            if (userCheck == null)
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = Errors.InvalidToken,
                    ErrorDescription = "The user does not exist."
                });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            var userCanSeeAllVehicles = await _authzService.AuthorizeAsync(
                   User, "ViewAllVehiclesPolicy");
            if (!userCanSeeAllVehicles.Succeeded) return Unauthorized();


            var userCanSeeAllUser = await _authzService.AuthorizeAsync(
                   User, "ViewAllVehiclesPolicy");
            if (!userCanSeeAllUser.Succeeded) return Unauthorized();

            if (!User.Identity.IsAuthenticated) return Unauthorized();

            var Vehicles = new PagedResults<Vehicle>();

            Vehicles = await _vehicleService.GetVehiclesForUserIdAsync(
                userId, pagingOptions, sortOptions, searchOptions);

            if (Vehicles == null) return NotFound();

            var collectionLink = Link.To(nameof(GetVehiclesForUser));
            var collection = PagedCollection<Vehicle>.Create<VehiclesResponse>(
                collectionLink,
                Vehicles.Items.ToArray(),
                Vehicles.TotalSize,
                pagingOptions);

            collection.VehicleQuery = FormMetadata.FromResource<Vehicle>(
                Link.ToForm(
                    nameof(GetVehiclesForUser),
                    null,
                    Link.GetMethod, null,
                    Form.QueryRelation));

            return Ok(collection);
        }
    }
}
