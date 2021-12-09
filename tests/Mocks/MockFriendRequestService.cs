using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;
using Api.Services;
using Api.Utils;

namespace Api.Tests.Mocks
{
    public class MockFriendRequestService : ServiceBase, IFriendRequestService
    {

        public MockFriendRequestService()
        {

        }

        public MockFriendRequestService(List<User> users)
        {
            usersStore = users;
        }

        public List<User> usersStore { get; set; } = new List<User>();

        public async Task<ServiceResult> AddFriendRequest(int userId, int friendId)
        {
            if(userId == friendId)
                return Conflict("Cannot add friend that has the same ID as a user");

            User user = usersStore.Find(x => x.Id == userId);
            User friend = usersStore.Find(x => x.Id == friendId);

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

            return Success();
        }

        public async Task<ServiceResult<List<User>>> GetFriendRequests(int userId)
        {
            User user = usersStore.Find(x => x.Id == userId);


            if (user == null)
                return new NotFoundServiceResult("User not found");

            return user.FriendRequests;
        }

        public async Task<ServiceResult> RemoveFriendRequest(int userId, int friendId)
        {
            User user = usersStore.Find(x => x.Id == userId);
            User friend = usersStore.Find(x => x.Id == friendId);

            if (user == null)
                return NotFound("User not found");
            if (friend == null)
                return NotFound("Friend not found");


            user.FriendRequests.Remove(friend);


            return Success();
        }
    }
}
