using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Api.Utils;

namespace Api.Services
{
    public interface IFriendRequestService
    {
        Task<ServiceResult<List<User>>> GetFriendRequests(int userId);
        Task<ServiceResult> AddFriendRequest(int userId, int friendId);
        Task<ServiceResult> RemoveFriendRequest(int userId, int friendId);
    }

    public class FriendRequestService : ServiceBase, IFriendRequestService
    {
        public readonly ApiContext _context;
        public FriendRequestService(ApiContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> AddFriendRequest(int userId, int friendId)
        {
            if(userId == friendId)
                return Conflict("Cannot add friend with same ID as a user");

            User user = await _context.Users
            .Include(x => x.FriendRequests)
            .Include(x => x.Friends)
            .FirstOrDefaultAsync(x => x.Id == userId);

            User friend = await _context.Users
            .Include(x => x.FriendRequests)
            .Include(x => x.Friends)
            .FirstOrDefaultAsync(x => x.Id == friendId);

            if (user == null)
                return NotFound("User not found");
            if (friend == null)
                return NotFound("Friend not found");

            //give friend request to a friend
            friend.FriendRequests.Add(user);
            if (user.FriendRequests.Contains(friend))
            {
                user.Friends.Add(friend);
                friend.Friends.Add(user);
                user.FriendRequests.Remove(friend);
                friend.FriendRequests.Remove(user);
            }
            await _context.SaveChangesAsync();

            return Success();
        }

        public async Task<ServiceResult<List<User>>> GetFriendRequests(int userId)
        {
            User user = await _context.Users
            .Include(x => x.FriendRequests)
            .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return new NotFoundServiceResult("User not found");

            return user.FriendRequests;
        }

        public async Task<ServiceResult> RemoveFriendRequest(int userId, int friendId)
        {
            User user = await _context.Users
            .Include(x => x.FriendRequests)
            .FirstOrDefaultAsync(x => x.Id == userId);

            User friend = await _context.Users
            .Include(x => x.FriendRequests)
            .FirstOrDefaultAsync(x => x.Id == friendId);

            if (user == null)
                return NotFound("User not found");
            if (friend == null)
                return NotFound("Friend not found");


            user.FriendRequests.Remove(friend);
            await _context.SaveChangesAsync();

            return Success();
        }
    }

}