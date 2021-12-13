using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;
using Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IRouteService
    {
        Task<Route> GetRouteById(int id);
        Task<ServiceResult<List<Route>>> GetUserRoutes(int userId);
        Task<ServiceResult> RemoveRouteById(int id);
        Task<ServiceResult> RemoveRoute(Route u);
        Task<ServiceResult> UpdateRoute(Route u);
        Task<ServiceResult> CreateRoute(Route u);
    }

    public class RouteService : ServiceBase, IRouteService
    {
        public readonly ApiContext _context;
        public RouteService(ApiContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> CreateRoute(Route route)
        {
            if (await _context.Users.FirstOrDefaultAsync(x => x.Id == route.UserId) == null)
                return NotFound("Cannot create route for non existing user");

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<Route> GetRouteById(int id)
        {
            var result = await _context.Routes.FindAsync(id);
            return result;
        }

        public async Task<ServiceResult<List<Route>>> GetUserRoutes(int userId)
        {
            var users = _context.Users
            .Include(x => x.Routes);

            var user = await users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return NotFound("User not found");
            return user.Routes;
        }

        public async Task<ServiceResult> RemoveRoute(Route route)
        {
            var check = await _context.Routes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == route.Id);
            if (check == null)
                return NotFound("Route not found");

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<ServiceResult> RemoveRouteById(int id)
        {
            Route route = await _context.Routes.FindAsync(id);
            if (route == null)
                return NotFound("Route not found");

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<ServiceResult> UpdateRoute(Route route)
        {
            var check = _context.Routes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == route.Id);

            var userCheck = _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == route.UserId);

            if (await check == null)
                return NotFound("Route not found");
            if (await userCheck == null)
                return NotFound("Cannot update route with non existing user as a owner");

            _context.Routes.Update(route);
            await _context.SaveChangesAsync();
            return Success();
        }
    }

}