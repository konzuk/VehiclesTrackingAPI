using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenIddict.Validation.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleTrackingAPI.Filters;
using VehicleTrackingAPI.Infrastructure;
using VehicleTrackingAPI.Models;
using VehicleTrackingAPI.Services;

namespace VehicleTrackingAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<AppOptions>(Configuration);
            services.Configure<PagingOptions>(
                   Configuration.GetSection("DefaultPagingOptions"));


            services.AddScoped<IUserService, DefaultUserService>();
            services.AddScoped<IVehicleService, DefaultVehicleService>();
            services.AddScoped<IPositionService, DefaultPositionService>();
            services.AddScoped<IPlaceService, DefaultPlaceService>();


            services.AddDbContext<VTApiDbContext>(
                options =>
                {

                    // Use in-memory database for quick dev and testing
                    // TODO: Swap out for a real database in production

                    options.UseInMemoryDatabase("7peakdb");

                    ////Change to SQLServer Code
                    //options.UseSqlServer(Configuration.GetConnectionString("7peakdbContext"), op => op.UseNetTopologySuite());


                    options.UseOpenIddict<Guid>();

                });

            // Call private functions to Config Identity Service.
            // Temporary using the same server. 
            // TODO: Swap out to difference Server. 
            AddIdentityCoreServices(services);



            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIddict.Abstractions.OpenIddictConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIddict.Abstractions.OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIddict.Abstractions.OpenIddictConstants.Claims.Role;
            });

            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                        .UseDbContext<VTApiDbContext>()
                        .ReplaceDefaultEntities<Guid>();
                })
                .AddServer(options =>
                {
                    // Enable the token endpoint.
                    options.SetTokenEndpointUris("/token");
                    // Enable the password flow.
                    options.AllowPasswordFlow(); 
                    // Accept anonymous clients (i.e clients that don't send a client_id).
                    options.AcceptAnonymousClients();
                    // Register the signing and encryption credentials.
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();
                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough();
                })
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });



           


           

            services
                .AddControllers(options =>
                {
                    options.CacheProfiles.Add("Static", new CacheProfile { Duration = 86400 });
                    options.CacheProfiles.Add("Collection", new CacheProfile { Duration = 60 });
                    options.CacheProfiles.Add("Resource", new CacheProfile { Duration = 180 });

                    options.Filters.Add<JsonExceptionFilter>();
                    options.Filters.Add<RequireHttpsOrCloseAttribute>();
                    options.Filters.Add<LinkRewritingFilter>();
                })
                .AddNewtonsoftJson(options =>
                {
                    // These should be the defaults, but we can be explicit:
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    options.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;

                });

            services.AddOpenApiDocument(config =>
            {
                config.SerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                config.PostProcess = document =>
                {
                    document.Info.Title = "Vehicle Tracking API";
                    document.Info.Description = "A DEMO/Test ASP.NET Core web API";
                    document.Info.TermsOfService = "None";
                    document.Info.Contact = new NSwag.OpenApiContact
                    {
                        Name = "Tan Hongyou",
                        Email = "tanhongyou@gmail.com"
                    };
                    
                };
            });
           


            services
                .AddRouting(options => options.LowercaseUrls = true);

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader
                    = new MediaTypeApiVersionReader();
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionSelector
                     = new CurrentImplementationApiVersionSelector(options);
            });

            services.AddAutoMapper(
               options => options.AddProfile<MappingProfile>());

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errorResponse = new ApiError(context.ModelState);
                    return new BadRequestObjectResult(errorResponse);
                };
            });

            services.AddResponseCaching();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ViewAllUsersPolicy",
                    p => p.RequireAuthenticatedUser().RequireRole("Admin"));

                options.AddPolicy("ViewAllVehiclesPolicy",
                    p => p.RequireAuthenticatedUser().RequireRole("Admin"));

                options.AddPolicy("ViewAllVehiclesPositionsPolicy",
                    p => p.RequireAuthenticatedUser().RequireRole("Admin"));

                options.AddPolicy("ViewAllPositionsPlacesPolicy",
                    p => p.RequireAuthenticatedUser().RequireRole("Admin"));

                options.AddPolicy("ViewAllVehiclesPositionPolicy",
                    p => p.RequireAuthenticatedUser().RequireRole("Admin"));

            }); 
            
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseOpenApi();
                app.UseSwaggerUi3();

            }
            else
            {
                app.UseHsts();
            }


            app.UseResponseCaching();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
            });
        }

        private static void AddIdentityCoreServices(IServiceCollection services)
        {
            var builder = services.AddIdentityCore<UserEntity>();

            builder = new IdentityBuilder(
                builder.UserType,
                typeof(UserRoleEntity),
                builder.Services);

            builder.AddRoles<UserRoleEntity>()
                .AddEntityFrameworkStores<VTApiDbContext>()
                .AddDefaultTokenProviders()
                .AddSignInManager<SignInManager<UserEntity>>();
        }
    }
}
