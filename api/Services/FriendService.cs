using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Api.Utils;
using System.Linq;

namespace Api.Services
{
    public interface IFriendService
    {
        Task<ServiceResult<List<User>>> GetFriends(int userId);
        Task<ServiceResult> SendFriendRequest(int requesterId, int receiverId);
        Task<ServiceResult> RemoveFriend(int userId, int friendId);
    }

    public class FriendService : ServiceBase, IFriendService
    {
        public readonly ApiContext _context;
        public FriendService(ApiContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> SendFriendRequest(int requesterId, int receiverId)
        {
            if (requesterId == receiverId)
                return Conflict("Cannot add friend that has the same ID as a user");

            User requester = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == requesterId);

            User receiver = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == receiverId);

            if (requester == null)
                return NotFound("User not found");
            if (receiver == null)
                return NotFound("Friend not found");


            var friendCheck = await _context.Friends.FirstOrDefaultAsync(x =>
            x.ReceiverId == receiverId && x.RequesterId == requesterId);

            var reverseFriendCheck = await _context.Friends.FirstOrDefaultAsync(x =>
             x.ReceiverId == requesterId && x.RequesterId == receiverId);

            if (friendCheck?.Status == AcceptanceStatus.Friends ||
            reverseFriendCheck?.Status == AcceptanceStatus.Friends)
                return Conflict($"{receiverId} already is friends with {requesterId}");


            if (friendCheck == null && reverseFriendCheck == null)
                await _context.Friends.AddAsync(new Friend()
                {
                    Date = System.DateTimeOffset.UtcNow,
                    ReceiverId = receiverId,
                    RequesterId = requesterId,
                    Status = AcceptanceStatus.Requested
                });

            if (reverseFriendCheck?.Status == AcceptanceStatus.Requested)
                reverseFriendCheck.Status = AcceptanceStatus.Friends;


            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<ServiceResult<List<User>>> GetFriends(int userId)
        {
            User user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound("User not found");

            var results = await _context.Friends
            .Where(x => (x.ReceiverId == userId || x.RequesterId == userId)
            && x.Status == AcceptanceStatus.Friends)
            .ToListAsync();

            return results.Select(x =>
            {
                if (x.ReceiverId == userId)
                    return x.Receiver;
                return x.Requester;
            }).ToList();
        }

        public async Task<ServiceResult> RemoveFriend(int userId, int friendId)
        {
            var friendQueryResult = await _context.Friends.FirstOrDefaultAsync(x =>
            (x.ReceiverId == userId && x.RequesterId == friendId) ||
            (x.RequesterId == userId && x.ReceiverId == friendId));

            if (friendQueryResult == null)
                return NotFound("Friend pair not found");

            _context.Friends.Remove(friendQueryResult);
            await _context.SaveChangesAsync();
            return Success();
        }
    }

}