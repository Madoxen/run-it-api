using Microsoft.Extensions.Options;

namespace Api.Configuration.Options
{
    public class AuthenticationOptions
    {
        public JWTOptions JWT { get; set; }
        public GoogleOptions Google { get; set; }
        public FacebookOptions Facebook { get; set; }
    }

    public class JWTOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public int ExpiryMinutes { get; set; }
    }

    public class GoogleOptions
    {
        public string ClientID { get; set; }
        public string IosID { get; set; }
    }

    public class FacebookOptions
    {
        public string AppID { get; set; }
        public string AppSecret { get; set; }
    }
}