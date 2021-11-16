using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Api.Models;

namespace Api.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        void AddFriend(int userId, int friendId);
        void DeleteFriend(int userId, int friendId);
    }
    public class UserRepository : EfCoreRepository<User, ApiContext>, IUserRepository
    {

        public UserRepository(ApiContext context) : base(context)
        {

        }

        public async Task<List<User>> GetFriends(int userId)
        {
            var user = await context.Users
            .Include(u => u.Friends).FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return null;

            return user.Friends;
        }

        public async void AddFriend(int userId, int friendId)
        {
            var user = await context.Users
            .Include(u => u.Friends).FirstOrDefaultAsync(x => x.Id == userId);

            var friend = await context.Users
            .Include(u => u.Friends).FirstOrDefaultAsync(x => x.Id == friendId);

            user.Friends.Add(friend);
            friend.Friends.Add(user);

            await context.SaveChangesAsync();
        }

        public async void DeleteFriend(int userId, int friendId)
        {
            var user = await context.Users
            .Include(u => u.Friends).FirstOrDefaultAsync(x => x.Id == userId);

            var friend = await context.Users
            .Include(u => u.Friends).FirstOrDefaultAsync(x => x.Id == friendId);

            user.Friends.Remove(friend);
            friend.Friends.Remove(user);

            await context.SaveChangesAsync();
        }
    }
}