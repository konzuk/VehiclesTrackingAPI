using VehicleTrackingAPI.Models;
using VehicleTrackingAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            
            var adminUser = userManager.Users
                .SingleOrDefault(u => u.Email == "admin@7peak.local");


            await context.SaveChangesAsync();
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



            // Add a test role
            await roleManager.CreateAsync(new UserRoleEntity("Admin2"));

        }
    }
}
