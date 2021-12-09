using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;
using Api.Services;
using Api.Utils;

namespace Api.Tests.Mocks
{
    public class MockFriendService : ServiceBase, IFriendService
    {

        public MockFriendService()
        {

        }

        public MockFriendService(List<User> users)
        {
            usersStore = users;
        }

        public List<User> usersStore { get; set; } = new List<User>();

        public async Task<ServiceResult> AddFriend(int userId, int friendId)
        {
            if(userId == friendId)
                return Conflict("Cannot add friend that has the same ID as a user");

            User user = usersStore.Find(x => x.Id == userId);
            User friend = usersStore.Find(x => x.Id == friendId);

            if (user == null)
                return NotFound("User not found");
            if (friend == null)
                return NotFound("Friend not found");

            user.Friends.Add(friend);
            friend.Friends.Add(user);

            return Success();
        }

        public async Task<ServiceResult<List<User>>> GetFriends(int userId)
        {
            User user = usersStore.Find(x => x.Id == userId);

            if (user == null)
                return new NotFoundServiceResult("User not found");

            return user.Friends;
        }

        public async Task<ServiceResult> RemoveFriend(int userId, int friendId)
        {
            User user = usersStore.Find(x => x.Id == userId);
            User friend = usersStore.Find(x => x.Id == friendId);

            if (user == null)
                return NotFound("User not found");
            if (friend == null)
                return NotFound("Friend not found");


            user.Friends.Remove(friend);
            friend.Friends.Remove(user);


            return Success();
        }
    }
}
