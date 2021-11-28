namespace Api.Models
{
    public struct MapPoint
    {
        double Longitude { get; set; }
        double Latitude { get; set; }

        public MapPoint(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
    }
}