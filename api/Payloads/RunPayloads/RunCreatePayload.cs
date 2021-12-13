using System;
using Api.Models;

namespace Api.Payloads
{
    public interface IRunCreatePayload : IModelPayload<Run>
    {
        int UserId { get; set; }
        string Title { get; set; }
        string Subtitle { get; set; }
        uint Duration { get; set; } //in seconds
        MapPoint[] Points { get; set; }
    }

    public class RunCreatePayload : IRunCreatePayload
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public uint Duration { get; set; } //in seconds
        public MapPoint[] Points { get; set; } //points tuple vector --- (lat, long)

        public Run CreateModel()
        {
            return new Run()
            {
                UserId = this.UserId,
                Title = this.Title,
                Subtitle = this.Subtitle,
                Duration = this.Duration,
                Points = this.Points
            };
        }
    }
}