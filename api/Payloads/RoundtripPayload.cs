using Api.Models;

namespace Api.Payloads
{
    public class RoundtripPayload
    {
        public MapPoint[] Points { get; set; }
        public double Distance { get; set; }
        public double ElevationDelta { get; set; }
    }
}