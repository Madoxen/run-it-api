using Api.Models;

namespace Api.Payloads
{
    public class UserGetPayload
    {
        public int Id { get; set; }
        public float? Weight { get; set; }
        public string Email { get; set; }
        public string GivenName { get; set; }
        public string LastName { get; set; }
        public uint DistanceLast30Days { get; set; }
        public uint DistanceTotal { get; set; }

        public UserGetPayload()
        {

        }

        public UserGetPayload(User user)
        {
            Id = user.Id;
            Weight = user.Weight;
            Email = user.Email;
            GivenName = user.GivenName;
            LastName = user.LastName;
            DistanceLast30Days = user.DistanceLast30Days;
            DistanceTotal = user.DistanceTotal;
        }

    }
}