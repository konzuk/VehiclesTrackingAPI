using AutoMapper;
using AutoMapper.QueryableExtensions;
using VehicleTrackingAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace VehicleTrackingAPI.Services
{
    public class DefaultPlaceService : IPlaceService
    {
        private readonly VTApiDbContext _context;
        private readonly IConfigurationProvider _mappingConfiguration;
        private readonly UserManager<UserEntity> _userManager;
        private readonly AppOptions _appOptions;


        public DefaultPlaceService(
            VTApiDbContext context,
            IOptions<AppOptions> optionsAccessor,
            IConfigurationProvider mappingConfiguration,
            UserManager<UserEntity> userManager)
        {
            _context = context;
            _mappingConfiguration = mappingConfiguration;
            _userManager = userManager;
            _appOptions = optionsAccessor.Value;
        }

        public async Task<Collection<Place>> GetPlaceForPositionAsync(Guid positionId)
        {
            var position = await _context.Positions
              .SingleOrDefaultAsync(r => r.Id == positionId);
            if (position == null) return null;

            // TODO: 
            //      1. After call Google API store it in Database. 
            //      2. Check if data already have in Database, use data in Database instead.
            //      3. Create and a new method to refresh Place Data 
            //            (Remove Place in Database and call API for to generate new place)
            return GoogleApi(position.Location.Y, position.Location.X);

        }


        private Collection<Place> GoogleApi(double latitude, double longitude)
        {
            string URL = _appOptions.GoogleReverseGeocodingURL;
            string result_type = _appOptions.GoogleAPIResultType;
            string key = _appOptions.GoogleAPIKey;
            string urlParameters = $"?latlng={latitude},{longitude}&result_type={result_type}&key={key}";

            string[] result_types = result_type.Split("|");

            var client = new RestClient($"{URL}{urlParameters}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = client.Execute(request);

            if (response.IsSuccessful)
            {
                // Parse the response body.
                var dataString = response.Content;
                JObject jsonObj = JObject.Parse(dataString);
                if (jsonObj != null)
                {
                    var status = (string)jsonObj["status"];

                    switch (status)
                    {
                        case "OK":

                            var results = jsonObj["results"];
                            var places = results.Select(s => new Place()
                            {
                                CreatedAt = DateTimeOffset.UtcNow,
                                place_id = (string)s["place_id"],
                                types = (string)s["types"].FirstOrDefault(s=> Array.IndexOf(result_types, (string)s) >= 0),
                                formatted_address = (string)s["formatted_address"]
                            }).ToArray();

                            var colPlaces = new Collection<Place>();
                            colPlaces.Value = places;

                            return colPlaces;
                        case "ZERO_RESULTS":
                            //indicates that the reverse geocoding was successful but returned no results. 
                            //This may occur if the geocoder was passed a latlng in a remote location.
                            return null;
                        case "OVER_QUERY_LIMIT":
                            //indicates that you are over your quota. 
                            return null;
                        case "REQUEST_DENIED":
                            //indicates that the request was denied. 
                            //Possibly because the request includes a result_type` or location_type` 
                            //parameter but does not include an API key or client ID.
                            return null;
                        case "INVALID_REQUEST":
                            //generally indicates one of the following:
                            //The query (address, components or latlng) is missing.
                            //An invalid result_type or location_type was given.
                            return null;
                        case "UNKNOWN_ERROR":
                        //indicates that the request could not be processed due to a server error. 
                        //The request may succeed if you try again.
                        default:
                            return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }
    }
}
