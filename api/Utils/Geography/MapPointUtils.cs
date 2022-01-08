using System;
using Api.Models;

namespace Utils.Geography
{
    public static class MapPointUtils
    {
        public static MapPoint[] GetMapPoints(byte[] rawPoints)
        {
            MapPoint[] result = new MapPoint[rawPoints.Length / 24];
            for (int i = 0; i < rawPoints.Length; i += 24)
            {
                double lat = BitConverter.ToDouble(rawPoints[i..(i + 8)]);
                double lon = BitConverter.ToDouble(rawPoints[(i + 8)..(i + 16)]);
                double height = BitConverter.ToDouble(rawPoints[(i + 16)..(i + 24)]);

                MapPoint p = new MapPoint(lon, lat, height);
                result[i / 24] = p;
            }
            return result;
        }

        public static byte[] GetRawPoints(MapPoint[] points)
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

        public static uint CalculateTotalDistance(MapPoint[] points)
        {
            if (points.Length < 2)
                return 0;

            double distanceTotal = 0;

            for (int i = 1; i < points.Length; i++)
            {
                MapPoint A = points[i - 1];
                MapPoint B = points[i];
                distanceTotal += DistanceCalculator.CalculateDistance(A, B);
            }

            return (uint)distanceTotal;
        }

        public static uint CalculateElevationDelta(MapPoint[] points)
        {
            if (points.Length < 2)
                return 0;

            uint heightMin = (uint)points[0].Height;
            uint heightMax = (uint)points[0].Height;

            for (int i = 1; i < points.Length; i++)
            {
                MapPoint A = points[i - 1];
                MapPoint B = points[i];

                if (heightMax < B.Height)
                    heightMax = (uint)B.Height;
                if (heightMin > B.Height)
                    heightMin = (uint)B.Height;
            }

            return heightMax - heightMin;
        }
    }
}