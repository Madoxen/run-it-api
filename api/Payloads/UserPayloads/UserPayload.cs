using Api.Models;

namespace Api.Payloads
{
        public class UserPayload : IModelPayload<User>
    {
        public int Id { get; set; }
        public float? Weight { get; set; }

        public virtual User CreateModel()
        {
            return new User()
            {
                Id = Id,
                Weight = Weight
            };
        }
    }
}