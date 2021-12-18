using System;
using Api.Models;

namespace Api.Payloads
{
    public class RouteCreatePayload : IModelPayload<Route> 
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public MapPoint[] Points { get; set; } //points tuple vector --- (lat, long)

        public virtual Route CreateModel()
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