using System;
using Api.Models;

namespace Api.Payloads
{
    public interface IRouteCreatePayload : IModelPayload<Route> {
        int UserId { get; set; }
        string Title { get; set; }
        string Subtitle { get; set; }
        MapPoint[] Points { get; set; }
    }
    public class RouteCreatePayload : IRouteCreatePayload
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public MapPoint[] Points { get; set; } //points tuple vector --- (lat, long)

        public Route CreateModel()
        {
            return new Route()
            {
                UserId = this.UserId,
                Title = this.Title,
                Subtitle = this.Subtitle,
                Points = this.Points
            };
        }
    }
}