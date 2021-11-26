using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Api.Models;
using Api.Services;
using Api.Utils;
using System;

namespace Api.Tests.Mocks
{
    public class MockUserAuthService : ServiceBase, IUserAuthService
    {

        public MockUserAuthService(List<User> users)
        {
            usersStore = users;
        }

        public MockUserAuthService()
        {

        }

        public List<User> usersStore { get; set; } = new List<User>();

        public async Task<User> GetUserByGoogleId(string googleId)
        {
            var result = usersStore.Find(x => x.GoogleId == googleId);
            return result;
        }

        public async Task<User> GetUserByFacebookId(string facebookId)
        {
            var result = usersStore.Find(x => x.FacebookId == facebookId);
            return result;
        }
    }
}
