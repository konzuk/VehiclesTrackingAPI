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
                    Link.GetMethod,null,
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
        [Authorize]
        [HttpGet("{userId}/listVehicles", Name = nameof(GetVehiclesForUser))]
        [ProducesResponseType(401)]
        [ProducesResponseType(200)]
        public async Task<PagedCollection<Vehicle>> GetVehiclesForUser(
            Guid userId,
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
                var userCanSeeAllUser = await _authzService.AuthorizeAsync(
                    User, "ViewAllUsersPolicy");
                if (userCanSeeAllVehicles.Succeeded && userCanSeeAllUser.Succeeded)
                {
                    var user = await _userService.GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        Vehicles = await _vehicleService.GetVehiclesForUserIdAsync(
                            userId, pagingOptions, sortOptions, searchOptions);
                    }
                }
              
            }

            var collectionLink = Link.To(nameof(GetVehiclesForUser));
            var collection = PagedCollection<Vehicle>.Create<PagedCollection<Vehicle>>(
                collectionLink,
                Vehicles.Items.ToArray(),
                Vehicles.TotalSize,
                pagingOptions);

            return collection;
        }
    }
}
