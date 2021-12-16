
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api.Payloads.Osrm
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Geometry
    {
        [JsonPropertyName("coordinates")]
        public List<List<double>> Coordinates { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class Leg
    {
        [JsonPropertyName("steps")]
        public List<object> Steps { get; set; }

        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("weight")]
        public double Weight { get; set; }
    }

    public class Trip
    {
        [JsonPropertyName("geometry")]
        public Geometry Geometry { get; set; }

        [JsonPropertyName("legs")]
        public List<Leg> Legs { get; set; }

        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("weight_name")]
        public string WeightName { get; set; }

        [JsonPropertyName("weight")]
        public double Weight { get; set; }
    }

    public class Waypoint
    {
        [JsonPropertyName("waypoint_index")]
        public int WaypointIndex { get; set; }

        [JsonPropertyName("trips_index")]
        public int TripsIndex { get; set; }

        [JsonPropertyName("location")]
        public List<double> Location { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("hint")]
        public string Hint { get; set; }
    }

    public class TripPayload
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("trips")]
        public List<Trip> Trips { get; set; }

        [JsonPropertyName("waypoints")]
        public List<Waypoint> Waypoints { get; set; }
    }

}
