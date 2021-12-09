using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Api.Utils;

namespace Api.Services
{
    public interface IFriendService
    {
        Task<ServiceResult<List<User>>> GetFriends(int userId);
        Task<ServiceResult> AddFriend(int userId, int friendId);
        Task<ServiceResult> RemoveFriend(int userId, int friendId);
    }

    public class FriendService : ServiceBase, IFriendService
    {
        public readonly ApiContext _context;
        public FriendService(ApiContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> AddFriend(int userId, int friendId)
        {
            if (userId == friendId)
                return Conflict("Cannot add friend that has the same ID as a user");

            User user = await _context.Users
            .Include(x => x.Friends)
            .FirstOrDefaultAsync(x => x.Id == userId);

            User friend = await _context.Users
            .Include(x => x.Friends)
            .FirstOrDefaultAsync(x => x.Id == friendId);

            if (user == null)
                return NotFound("User not found");
            if (friend == null)
                return NotFound("Friend not found");

            if (user.Friends.Contains(friend))
                return Conflict($"{friendId} already is friends with {userId}");

            user.Friends.Add(friend);
            friend.Friends.Add(user);
            await _context.SaveChangesAsync();

            return Success();
        }

        public async Task<ServiceResult<List<User>>> GetFriends(int userId)
        {
            User user = await _context.Users
            .Include(x => x.Friends)
            .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return new NotFoundServiceResult("User not found");

            return user.Friends;
        }

        public async Task<ServiceResult> RemoveFriend(int userId, int friendId)
        {
            User user = await _context.Users
            .Include(x => x.Friends)
            .FirstOrDefaultAsync(x => x.Id == userId);

            User friend = await _context.Users
            .Include(x => x.Friends)
            .FirstOrDefaultAsync(x => x.Id == friendId);

            if (user == null)
                return NotFound("User not found");
            if (friend == null)
                return NotFound("Friend not found");


            user.Friends.Remove(friend);
            friend.Friends.Remove(user);
            await _context.SaveChangesAsync();

            return Success();
        }
    }

}