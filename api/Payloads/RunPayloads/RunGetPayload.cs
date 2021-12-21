using System;
using Api.Models;

namespace Api.Payloads
{
    public class RunGetPayload
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public uint Duration { get; set; } //in seconds
        public uint DistanceTotal { get; set; }
        public uint ElevationDelta { get; set; }
        public DateTimeOffset Date { get; set; }
        public MapPoint[] Points { get; set; } //points tuple vector --- (lat, long)

        public RunGetPayload()
        { }

        public RunGetPayload(Run model)
        {
            Id = model.Id;
            UserId = model.UserId;
            Title = model.Title;
            Subtitle = model.Subtitle;
            Duration = model.Duration;
            Date = model.Date;
            Points = model.Points;
            DistanceTotal = model.DistanceTotal;
            ElevationDelta = model.ElevationDelta;
        }
    }
}