using System;
using Api.Models;

namespace Api.Payloads
{
    public class RunUpdatePayload : IModelPayload<Run>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public uint Duration { get; set; } //in seconds
        public MapPoint[] Points { get; set; } //points tuple vector --- (lat, long)

        public Run CreateModel()
        {
            return new Run()
            {
                Id = this.Id,
                UserId = this.UserId,
                Title = this.Title,
                Subtitle = this.Subtitle,
                Duration = this.Duration,
                Points = this.Points
            };
        }
    }
}