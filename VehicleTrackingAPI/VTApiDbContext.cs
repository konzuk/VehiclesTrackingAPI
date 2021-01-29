using VehicleTrackingAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI
{
    public class VTApiDbContext : IdentityDbContext<UserEntity, UserRoleEntity, Guid>
    {
        public VTApiDbContext(DbContextOptions options)
            : base(options) { }

        public DbSet<VehicleEntity> Vehicles { get; set; }
        public DbSet<PositionEntity> Positions { get; set; }

    }
}
