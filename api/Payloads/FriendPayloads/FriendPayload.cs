using Api.Models;

namespace Api.Payloads
{
    public class FriendPayload : IModelPayload<User>
    {
        public int Id { get; set; }
        public string GivenName { get; set; }
        public string LastName { get; set; }

        public FriendPayload(User model)
        {
            Id = model.Id;
            GivenName = model.GivenName;
            LastName = model.LastName;
        }

        public virtual User CreateModel()
        {
            throw new System.NotImplementedException();
        }
    }
}