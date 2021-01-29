using AutoMapper;
using VehicleTrackingAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTrackingAPI.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
           

            CreateMap<UserEntity, User>()
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.UsersController.GetUserById),
                    new { userId = src.Id })));

            CreateMap<VehicleEntity, Vehicle>()
                 .ForMember(dest => dest.User, opt => opt.MapFrom(src =>
                     Link.To(nameof(Controllers.UsersController.GetUserById),
                             new { userId = src.User.Id })))
                 .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                     Link.To(
                         nameof(Controllers.VehiclesController.GetVehicleById),
                         new { VehicleId = src.Id })))
                 .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                     $"{src.User.FirstName} {src.User.LastName}"));
        }
    }
}
