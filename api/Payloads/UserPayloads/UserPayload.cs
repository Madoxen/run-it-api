using Api.Models;

namespace Api.Payloads
{
    public interface IUserPayload : IModelPayload<User>
    {
        int Id { get; set; }
        float? Weight { get; set; }
    }
    public class UserPayload : IUserPayload
    {
        public int Id { get; set; }
        public float? Weight { get; set; }

        public User CreateModel()
        {
            return new User()
            {
                Id = Id,
                Weight = Weight
            };
        }
    }
}