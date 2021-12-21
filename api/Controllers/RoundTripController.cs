using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Api.Models;
using Api.Handlers;
using Api.Payloads;
using System.Linq;
using Api.Services;
using Api.Utils;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Api.Payloads.Osrm;
using Utils.Geography;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class RoundTripController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly Random _randomizer;

        public RoundTripController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _randomizer = new Random();
        }

        [HttpGet]
        [Route("{longitude}&{latitude}&{length}")]
        [AllowAnonymous]
        public async Task<ActionResult<RoundtripPayload>> Get(double longitude, double latitude, double length)
        {
            var httpClient = _clientFactory.CreateClient();
            //GET /trip/v1/{profile}/{coordinates}?
            //roundtrip={true|false}&
            //source{any|first}&
            //destination{any|last}&
            //steps={true|false}&
            //geometries={polyline|polyline6|geojson}&
            //overview={simplified|full|false}&
            //annotations={true|false}'
            var circleSampleCoords = CreateCircleCoordinateArray(new MapPoint(longitude, latitude), length);
            StringBuilder queryStringBuilder = new StringBuilder();
            for (int i = 0; i < circleSampleCoords.Length; i++)
            {
                var point = circleSampleCoords[i];
                queryStringBuilder.Append((decimal)point.Longitude);
                queryStringBuilder.Append(',');
                queryStringBuilder.Append((decimal)point.Latitude);
                if (i == circleSampleCoords.Length - 1)
                    break;
                queryStringBuilder.Append(';');
            }

            var coordQuery = queryStringBuilder.ToString();

            var response = await httpClient.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://roundtrip:5000/trip/v1/foot/{coordQuery}?overview=full&geometries=geojson&roundtrip=true")
            });

            if (!response.IsSuccessStatusCode)
                return NotFound("Error in OSRM api");

            using var responseStream = await response.Content.ReadAsStreamAsync();

            var payload = await JsonSerializer.DeserializeAsync<TripPayload>(responseStream);
            if (payload.Trips.Count <= 0)
                return NotFound("Could not find any matching route");
            var pointArray = payload.Trips[0].Geometry.Coordinates.Select(x => new MapPoint(x[0], x[1]));

            var result = new RoundtripPayload();
            result.Points = pointArray.ToArray();
            result.Distance = MapPointUtils.CalculateTotalDistance(result.Points);
            result.ElevationDelta = MapPointUtils.CalculateElevationDelta(result.Points);
            return result;
        }


        //radius in km
        //angle in radius
        //resoulution in - n points
        private MapPoint[] CreateCircleCoordinateArray(MapPoint startCoords, double trackLength, int resolution = 4)
        {
            double wallLength = trackLength / resolution; //track length is approx. a generated circle circumference
            double radius = Math.Sqrt((wallLength * wallLength) / (2 * (1 - Math.Cos(2 * Math.PI / resolution))));
            double rotation = 2.0 * Math.PI * _randomizer.NextDouble();
            //TODO: calculate resolution based on radius
            MapPoint[] result = new MapPoint[resolution + 1];
            for (int i = 0; i < resolution; i++)
            {
                double angle = 2.0 * Math.PI * ((double)i / (double)resolution); //current sampling angle in radians 

                double x = radius * (Math.Cos(angle) + Math.Cos(rotation));
                double y = radius * (Math.Sin(angle) + Math.Sin(rotation));
                double heading = Math.Atan2(y, x);
                double distance = Math.Sqrt(x * x + y * y);
                MapPoint p = DistanceCalculator.GetPointByDistanceAndHeading(startCoords, heading, distance);
                p.Latitude = Math.Round(p.Latitude, 6);
                p.Longitude = Math.Round(p.Longitude, 6);
                result[i] = p;
            }
            result[resolution] = result[0];
            return result;
        }
    }
}
