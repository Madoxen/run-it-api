using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Api.Models
{
    //Singular training
    public class Run : IEntity
    {
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
        public DateTime Date { get; set; }
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
                    _points = GetMapPoints();
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
                RawPoints = GetRawPoints(value);
                UpdateComputedValues(value);
            }
        }

        private MapPoint[] GetMapPoints()
        {
            MapPoint[] result = new MapPoint[RawPoints.Length / 24];
            for (int i = 0; i < RawPoints.Length; i += 24)
            {
                double lat = BitConverter.ToDouble(RawPoints[i..(i+8)]);
                double lon = BitConverter.ToDouble(RawPoints[(i + 8)..(i+16)]);
                double height = BitConverter.ToDouble(RawPoints[(i + 16)..(i+24)]);

                MapPoint p = new MapPoint(lat, lon, height);
                result[i/24] = p;
            }
            return result;
        }

        private byte[] GetRawPoints(MapPoint[] points)
        {
            byte[] result = new byte[points.Length * 24];
            for (int i = 0; i < points.Length; i++)
            {
                MapPoint current = points[i];
                int offset = i * 24;
                byte[] latBytes = BitConverter.GetBytes(current.Latitude);
                byte[] lonBytes = BitConverter.GetBytes(current.Longitude);
                byte[] heightBytes = BitConverter.GetBytes(current.Height);

                latBytes.CopyTo(result, offset + 0);
                lonBytes.CopyTo(result, offset + 8);
                heightBytes.CopyTo(result, offset + 16);
            }
            return result;
        }

        private void UpdateComputedValues(MapPoint[] points)
        {
            double distanceTotal = 0;
            uint heightMin = (uint)points[0].Height;
            uint heightMax = (uint)points[0].Height;
            for (int i = 1; i < points.Length; i++)
            {
                MapPoint A = points[i - 1];
                MapPoint B = points[i];
                distanceTotal += DistanceCalculator.CalculateDistance(A, B);

                if (heightMax < B.Height)
                    heightMax = (uint)B.Height;
                if (heightMin > B.Height)
                    heightMin = (uint)B.Height;
            }
            ElevationDelta = heightMax - heightMin;
            DistanceTotal = (uint)distanceTotal;
        }

    }
}