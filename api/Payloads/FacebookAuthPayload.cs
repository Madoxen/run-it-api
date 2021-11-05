using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api.Payloads
{
    //Payload from debug_token
    public class FacebookAuthPayload
    {
        [JsonPropertyName("data")]
        public Data PayloadData { get; set; }

        public class Data
        {
            [JsonPropertyName("app_id")]
            public string AppId { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("application")]
            public string Application { get; set; }

            [JsonPropertyName("data_access_expires_at")]
            public int DataAccessExpiresAt { get; set; }

            [JsonPropertyName("expires_at")]
            public int ExpiresAt { get; set; }

            [JsonPropertyName("is_valid")]
            public bool IsValid { get; set; }

            [JsonPropertyName("scopes")]
            public List<string> Scopes { get; set; }

            [JsonPropertyName("user_id")]
            public string UserId { get; set; }
        }
    }
}