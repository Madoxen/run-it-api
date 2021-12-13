using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Utils.Geography;

namespace Api.Models
{
    //Singular training
    public class Run : IEntity
    {
        //TODO: Make Id and UserId required for this model
        public Run() { }

        [Key]
        public int Id { get; set; }

        //Relationships
        //Ownership
        public int UserId { get; set; }
        public User User { get; set; }

        //Data
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public uint Duration { get; set; } //in seconds
        public uint ElevationDelta { get; private set; } //in meters
        public uint DistanceTotal { get; private set; } //in meters
        public DateTimeOffset Date { get; set; }
        public byte[] RawPoints { get; private set; } //points tuple vector --- (lat, long, height) 24bytes each section

        [NotMapped]
        private MapPoint[] _points;
        [NotMapped]
        public MapPoint[] Points
        {
            get
            {
                if (RawPoints == null)
                    return null;
                if (_points == null)
                    _points = MapPointUtils.GetMapPoints(RawPoints);
                return _points;
            }
            set
            {
                if (value == null)
                {
                    RawPoints = null;
                    DistanceTotal = 0;
                    ElevationDelta = 0;
                    return;
                }
                _points = value;
                RawPoints = MapPointUtils.GetRawPoints(value);
                DistanceTotal = MapPointUtils.CalculateTotalDistance(value);
                ElevationDelta = MapPointUtils.CalculateElevationDelta(value);
            }
        }
    }
}