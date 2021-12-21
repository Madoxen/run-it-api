using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;
using Api.Utils;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Api.Services
{
    public interface IRouteService
    {
        Task<Route> GetRouteById(int id);
        Task<ServiceResult<List<Route>>> GetUserRoutes(int userId);
        Task<ServiceResult> RemoveRouteById(int id);
        Task<ServiceResult> RemoveRoute(Route u);
        Task<ServiceResult> UpdateRoute(Route u);
        Task<ServiceResult<Route>> CreateRoute(Route u);
    }

    public class RouteService : ServiceBase, IRouteService
    {
        public readonly ApiContext _context;
        public RouteService(ApiContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<Route>> CreateRoute(Route route)
        {
            if (await _context.Users.FirstOrDefaultAsync(x => x.Id == route.UserId) == null)
                return NotFound("Cannot create route for non existing user");

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return route;
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

        public async Task<ServiceResult> ShareRouteWith(int routeId, int shareToId)
        {
            var check = _context.Routes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == routeId);

            var shareCheck = _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == shareToId);

            if (await check == null)
                return NotFound("Route not found");
            if (await shareCheck == null)
                return NotFound("User not found");

            await _context.RouteShares.AddAsync(new RouteShare() { RouteId = routeId, SharedToId = shareToId, Date = System.DateTimeOffset.UtcNow });
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<ServiceResult> RemoveShare(int routeId, int shareToId)
        {
            var share = await _context.RouteShares.FirstOrDefaultAsync(x => x.RouteId == routeId && x.SharedToId == shareToId);

            if (share == null)
                return NotFound("RouteShare not found");

            _context.RouteShares.Remove(share);
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<ServiceResult<List<Route>>> GetSharesForUser(int userId)
        {
            var userCheck = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

            if (userCheck == null)
                return NotFound("User not found");

            return _context.RouteShares.AsNoTracking().Where(x => x.SharedToId == userId).Select(x => x.Route).ToList();
        }
    }
}