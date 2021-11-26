using System.Threading.Tasks;
using Api.Models;
using Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IUserAuthService

    {
        Task<User> GetUserByGoogleId(string googleId);
        Task<User> GetUserByFacebookId(string facebookId);

    }

    public class UserAuthService : ServiceBase, IUserAuthService
    {
        public readonly ApiContext _context;
        public UserAuthService(ApiContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByFacebookId(string facebookId)
        {
            var result = await _context.Users.FirstOrDefaultAsync(x => x.FacebookId == facebookId);
            return result;
        }

        public async Task<User> GetUserByGoogleId(string googleId)
        {
            var result = await _context.Users.FirstOrDefaultAsync(x => x.GoogleId == googleId);
            return result;
        }
    }

}