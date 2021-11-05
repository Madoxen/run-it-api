namespace Api.Payloads
{
    public class JWTAuthPayload
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int user_id { get; set; }
    }
}