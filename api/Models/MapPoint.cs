namespace Api.Models
{
    public struct MapPoint
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Height { get; set; }

        public MapPoint(double longitude, double latitude, double height = 0)
        {
            Longitude = longitude;
            Latitude = latitude;
            Height = height;
        }
    }
}