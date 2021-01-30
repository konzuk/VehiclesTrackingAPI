using VehicleTrackingAPI.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Models
{
    public class PositionRegisterForm
    {
        [Required]
        [Range(-90, 90)]
        [Display(Name = "latitude", Description = "Latitude")]
        public double Lat { get; set; }


        [Required]
        [Range(-180,180)]
        [Display(Name = "longitude", Description = "Longitude")]
        public double Long { get; set; }

    }

}
