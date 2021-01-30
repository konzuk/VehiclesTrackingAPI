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
                    new { userId = src.Id })))
                .ForMember(dest => dest.ListVehicles, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.UsersController.GetVehiclesForUser),
                    new { userId = src.Id })));

            CreateMap<VehicleEntity, Vehicle>()
                 .ForMember(dest => dest.User, opt => opt.MapFrom(src =>
                     Link.To(nameof(Controllers.UsersController.GetUserById),
                             new { userId = src.User.Id })))
                 .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                     Link.To(nameof(Controllers.VehiclesController.GetVehicleById),
                         new { vehicleId = src.Id })))
                 .ForMember(dest => dest.Postions, opt => opt.MapFrom(src =>
                     Link.To(nameof(Controllers.VehiclesController.GetPositionsForVehicle),
                         new { vehicleId = src.Id })))
                 .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                     $"{src.User.FirstName} {src.User.LastName}"))
                 .ForMember(dest => dest.CurrentPositions, opt => opt.MapFrom(src =>
                     Link.To(nameof(Controllers.VehiclesController.GetCurrPositionByVehicleId),
                             new { vehicleId = src.Id})))
                 .ForMember(dest => dest.RegisterPosition, opt => opt.MapFrom(src =>
                    FormMetadata.FromModel(new PositionRegisterForm()
                    {
                        Lat = 0,
                        Long = 0
                    },
                        Link.ToForm(
                            nameof(Controllers.VehiclesController.CreatePositionForVehicle),
                            new { vehicleId = src.Id },
                            Link.PostMethod,
                            Link.JsonMediaType,
                            Form.CreateRelation))));


            CreateMap<PositionEntity, Position>()
                .ForMember(dest => dest.Place, opt => opt.MapFrom(src =>
                     Link.To(nameof(Controllers.PositionsController.GetPlaceForPosition),
                         new { positionId = src.Id })))
                 .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src =>
                     $"{src.Location.Y}"))
                 .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src =>
                     $"{src.Location.X}"));
        }
    }
}
