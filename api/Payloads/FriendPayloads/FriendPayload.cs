using Api.Models;

namespace Api.Payloads
{
    public class FriendPayload
    {
        public int Id { get; set; }
        public string GivenName { get; set; }
        public string LastName { get; set; }
        public uint DistanceLast30Days { get; set; }

        public FriendPayload(User model)
        {
            Id = model.Id;
            GivenName = model.GivenName;
            LastName = model.LastName;
            DistanceLast30Days = model.DistanceLast30Days;
        }
    }
}