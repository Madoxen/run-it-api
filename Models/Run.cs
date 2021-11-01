using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Api.Models
{
    //Singular training
    public class Run : IEntity
    {
        [Key]
        public int Id { get; set; }
        public uint Duration { get; set; } //in seconds
        public uint ElevationDelta { get; set; } //in meters
        public byte[] Points { get; set; } //points tuple vector --- (lat, long)
    }
}