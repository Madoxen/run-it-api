using System;
using Api.Models;

namespace Api.Payloads
{
    public class RouteGetPayload : IModelPayload<Route>
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public uint DistanceTotal { get; set; }
        public uint ElevationDelta { get; set; }
        public MapPoint[] Points { get; set; } //points tuple vector --- (lat, long)

        public RouteGetPayload()
        { }

        public RouteGetPayload(Route model)
        {
            UserId = model.UserId;
            Title = model.Title;
            Subtitle = model.Subtitle;
            Points = model.Points;
            DistanceTotal = model.DistanceTotal;
            ElevationDelta = model.ElevationDelta;
        }

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