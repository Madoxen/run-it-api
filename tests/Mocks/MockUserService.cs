using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Api.Models;
using Api.Services;
using Api.Utils;
using System;

namespace Api.Tests.Mocks
{
    public class MockUserService : ServiceBase, IUserService
    {

        public MockUserService(List<User> users)
        {
            usersStore = users;
        }

        public MockUserService()
        {
        }

        public List<User> usersStore { get; set; } = new List<User>();


        public async Task<User> GetUserById(int id)
        {
            var result = usersStore.Find(x => x.Id == id);
            return result;
        }

        public async Task<ServiceResult> RemoveUserById(int id)
        {
            User user = usersStore.Find(x => x.Id == id);
            if (user == null)
                return NotFound("User not found");
            usersStore.Remove(user);
            return Success();
        }

        public async Task<ServiceResult> RemoveUser(User user)
        {
            usersStore.Remove(user);
            return Success();
        }

        public async Task<ServiceResult> UpdateUser(User u)
        {
            int userIndex = usersStore.FindIndex(x => x.Id == u.Id);
            if (userIndex < 0)
                return NotFound("User not found");
            usersStore[userIndex] = u;
            return Success();
        }

        public void CreateUser(User u)
        {
            usersStore.Add(u);
        }
    }
}
