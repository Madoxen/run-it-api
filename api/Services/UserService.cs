using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IUserService
    {
        Task<User> GetUserById(int id);
        Task<ServiceResult> RemoveUserById(int id);
        Task<ServiceResult> RemoveUser(User u);
        Task<ServiceResult> UpdateUser(User u);
        Task<ServiceResult> UpdateUserRunStats(int userId);
        Task<User> CreateUser(User u);
    }

    public class UserService : ServiceBase, IUserService
    {
        public readonly ApiContext _context;
        public UserService(ApiContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserById(int id)
        {
            var result = await _context.Users.FindAsync(id);
            return result;
        }

        public async Task<ServiceResult> RemoveUserById(int id)
        {
            User user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<ServiceResult> RemoveUser(User user)
        {
            User u = await _context.Users.
            AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == user.Id);
            if (u == null)
                return NotFound("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<ServiceResult> UpdateUser(User u)
        {
            User user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == u.Id);
            if (user == null)
                return NotFound("User not found");
            _context.Users.Update(u);
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<User> CreateUser(User u)
        {
            await _context.Users.AddAsync(u);
            await _context.SaveChangesAsync();
            return u;
        }

        public async Task<ServiceResult> UpdateUserRunStats(int userId)
        {
            var user = await _context.Users
            .Include(x => x.Runs)
            .FirstOrDefaultAsync();

            if (user == null)
                return NotFound($"User {userId} not found");

            uint distanceTotal = 0;
            uint distanceLast30Days = 0;
            DateTimeOffset monthAgo = DateTimeOffset.UtcNow.AddMonths(-1);
            foreach (Run r in user.Runs)
            {
                if (r.Date > monthAgo)
                    distanceLast30Days += r.DistanceTotal;
                distanceTotal += r.DistanceTotal;
            }

            user.DistanceLast30Days = distanceLast30Days;
            user.DistanceTotal = distanceTotal;

            await _context.SaveChangesAsync();
            return Success();
        }
    }
}