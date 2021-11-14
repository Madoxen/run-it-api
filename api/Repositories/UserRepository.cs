using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        void AddFriend(User user, User friend);
        void DeleteFriend(User user, User friend);
    }
    public class UserRepository : EfCoreRepository<User, ApiContext>, IUserRepository
    {
        public UserRepository(ApiContext context) : base(context)
        {

        }

        public async void AddFriend(User user, User friend)
        {
            user.Friends.Add(friend);
            friend.Friends.Add(user);
            await context.SaveChangesAsync();
        }

        public async void DeleteFriend(User user, User friend)
        {
            user.Friends.Remove(friend);
            friend.Friends.Remove(friend);
            await context.SaveChangesAsync();
        }
    }
}