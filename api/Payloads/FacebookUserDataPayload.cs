
using System.Text.Json.Serialization;

namespace Api.Payloads
{
    public class FacebookUserDataPayload
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}