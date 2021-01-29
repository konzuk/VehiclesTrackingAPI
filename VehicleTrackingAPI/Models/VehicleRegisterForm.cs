using VehicleTrackingAPI.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Models
{
    public class VehicleRegisterForm
    {
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        [Display(Name = "name", Description = "Vehicle Name")]
        public string Name { get; set; }


        [MinLength(1)]
        [MaxLength(200)]
        [Display(Name = "Description", Description = "Vehicle Description")]
        public string Description { get; set; }
    }

}
