using System;
using Api.Models;

namespace Api.Payloads
{
    public class RouteUpdatePayload : IModelPayload<Route>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public MapPoint[] Points { get; set; } //points tuple vector --- (lat, long)

        public Route CreateModel()
        {
            return new Route()
            {
                Id = this.Id,
                UserId = this.UserId,
                Title = this.Title,
                Subtitle = this.Subtitle,
                Points = this.Points
            };
        }
    }
}