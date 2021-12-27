using Api.Models;
using System;

public static class DistanceCalculator
{
    public const double EARTH_RADIUS = 6378000;
    //Calculate distance between two points on earth map, in meters
    public static double CalculateDistance(MapPoint a, MapPoint b)
    {
        double a_long = a.Longitude * (Math.PI / 180);
        double a_lat = a.Latitude * (Math.PI / 180);

        double b_long = b.Longitude * (Math.PI / 180);
        double b_lat = b.Latitude * (Math.PI / 180);


        double deltaLat = Math.Abs(a_lat - b_lat);
        double deltaLong = Math.Abs(a_long - b_long);

        //https://en.wikipedia.org/wiki/Great-circle_distance  *Haversine 
        double angle = Math.Sqrt(
            Math.Pow(Math.Cos(b_lat) * Math.Sin(deltaLong), 2.0)
            + Math.Pow(Math.Cos(a_lat) * Math.Sin(b_lat) - Math.Sin(a_lat) * Math.Cos(b_lat) * Math.Cos(deltaLong), 2.0))
            / (Math.Sin(a_lat) * Math.Sin(b_lat) + Math.Cos(a_lat) * Math.Cos(b_lat) * Math.Cos(deltaLong));

        return Math.Atan(angle) * EARTH_RADIUS;
    }

    public static readonly double EarthRadius = 6378.1; //#Radius of the Earth km
    public static MapPoint GetPointByDistanceAndHeading(MapPoint origin, double heading, double distanceKm)
    {
        double bearingR = heading;
        double latR = origin.Latitude.ToRadians();
        double lonR = origin.Longitude.ToRadians();

        double distanceToRadius = distanceKm / EarthRadius;

        double newLatR = Math.Asin(Math.Sin(latR) * Math.Cos(distanceToRadius)
        + Math.Cos(latR) * Math.Sin(distanceToRadius) * Math.Cos(bearingR));

        double newLonR = lonR + Math.Atan2(
        Math.Sin(bearingR) * Math.Sin(distanceToRadius) * Math.Cos(latR),
        Math.Cos(distanceToRadius) - Math.Sin(latR) * Math.Sin(newLatR));

        return new MapPoint(newLonR.ToDegrees(), newLatR.ToDegrees());
    }

    public static double ToRadians(this double degrees)
    {
        return (Math.PI / 180.0) * degrees;
    }
    public static double ToDegrees(this double radians)
    {
        return (180.0 / Math.PI) * radians;
    }
}