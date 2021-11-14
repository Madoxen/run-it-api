using Api.Models;

namespace Api.Payloads
{
    public class FriendPayload 
    {
        public int Id { get; set; }

        public FriendPayload(User model)
        {
            Id = model.Id;
        }
    }
}