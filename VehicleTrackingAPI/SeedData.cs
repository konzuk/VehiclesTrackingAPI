using VehicleTrackingAPI.Models;
using VehicleTrackingAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace VehicleTrackingAPI
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            await AddAdminUsers(
                services.GetRequiredService<RoleManager<UserRoleEntity>>(),
                services.GetRequiredService<UserManager<UserEntity>>());

            await AddTestData(
                services.GetRequiredService<VTApiDbContext>(),
                services.GetRequiredService<UserManager<UserEntity>>());
        }

        public static async Task AddTestData(
            VTApiDbContext context,
            UserManager<UserEntity> userManager)
        {

            if (context.Vehicles.Any())
            {
                // Already has data
                return;
            }


            //for (int i = 0; i < 200; i++)
            //{
            //    var userEn = new UserEntity
            //    {
            //        Email = $"test_{i}_@local.com",
            //        UserName = $"test_{i}_@local.com",
            //        FirstName = $"test_{i}",
            //        LastName = $"test_{i}",
            //        CreatedAt = DateTimeOffset.UtcNow.AddDays(-200),
            //    };
            //    await userManager.CreateAsync(userEn, "SuperSecret123!!");
            //}

            //var users = context.Users.Where(s => s.FirstName.Contains("test_")).ToArray();

            //foreach (var user in users)
            //{
            //    for (int j = 0; j < 100; j++)
            //    {
            //        var id = Guid.NewGuid();

            //        var vi = new VehicleEntity
            //        {
            //            Id = id,
            //            CreatedAt = DateTimeOffset.UtcNow.AddDays(-200),
            //            ModifiedAt = DateTimeOffset.UtcNow.AddDays(-200),
            //            User = user,
            //            Name = $"test_vhi_{j}_{user.FirstName}",
            //            Description = $"test_vhi_{j}_{user.FirstName}",
            //        };
            //        context.Vehicles.Add(vi);
            //    }
            //    await context.SaveChangesAsync();
            //}

            //var vhs = context.Vehicles.ToArray();

            //var count = 1;


            //foreach (var vh in vhs)
            //{
                
            //    for (int k = 0; k < 100; k++)
            //    {
            //        var idPo = Guid.NewGuid();
            //        context.Positions.Add(new PositionEntity
            //        {
            //            Id = idPo,
            //            CreatedAt = DateTimeOffset.UtcNow.AddDays(-200).AddSeconds(30 * k),
            //            Vehicle = vh,
            //            Location = new Point(-122.333056, 47.609722) { SRID = 4326 }
            //        });
            //    }

            //    if (count == 1000)
            //    {
            //        await context.SaveChangesAsync();
            //        count = 1;
            //    }
            //    else 
            //    {
            //        count++;
            //    }
            //}

        }

        private static async Task AddAdminUsers(
            RoleManager<UserRoleEntity> roleManager,
            UserManager<UserEntity> userManager)
        {
            var dataExists = roleManager.Roles.Any() || userManager.Users.Any();
            if (dataExists)
            {
                return;
            }

            // Add a test role
            await roleManager.CreateAsync(new UserRoleEntity("Admin"));

            // Add a test user
            var user = new UserEntity
            {
                Email = "admin@7peak.local",
                UserName = "admin@7peak.local",
                FirstName = "Admin",
                LastName = "Tester",
                CreatedAt = DateTimeOffset.UtcNow
            };

            await userManager.CreateAsync(user, "SuperSecret123!!");

            // Put the user in the admin role
            await userManager.AddToRoleAsync(user, "Admin");
            await userManager.UpdateAsync(user);

        }
    }
}
