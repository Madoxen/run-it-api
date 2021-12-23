using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IRouteShareService
    {
        Task<ServiceResult> ShareRouteWith(int routeId, int shareToId);
        Task<ServiceResult> AcceptShare(int routeId, int shareToId);
        Task<ServiceResult> RemoveShare(int routeId, int shareToId);
        Task<ServiceResult<List<RouteShare>>> GetSharesForUser(int userId);
        Task<ServiceResult<List<RouteShare>>> GetShareRequestsForUser(int userId);
        Task<RouteShare> GetRouteShare(int routeId, int userId);
    }
    public class RouteShareService : ServiceBase, IRouteShareService
    {

        private readonly ApiContext _context;
        public RouteShareService(ApiContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> ShareRouteWith(int routeId, int shareToId)
        {
            var routeCheck = await _context.Routes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == routeId);

            var userCheck = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == shareToId);

            if (routeCheck == null)
                return NotFound("Route not found");
            if (userCheck == null)
                return NotFound("User not found");

            var shareCheck = await _context.RouteShares.FirstOrDefaultAsync(x => x.RouteId == routeId && x.SharedToId == shareToId);
            if (shareCheck != null)
                return Conflict("RouteShare already exists");
            await _context.RouteShares.AddAsync(new RouteShare()
            {
                RouteId = routeId,
                SharedToId = shareToId,
                Date = System.DateTimeOffset.UtcNow,
                Status = RouteShare.AcceptanceStatus.Sent
            });
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<ServiceResult> AcceptShare(int routeId, int shareToId)
        {
            var share = await _context.RouteShares.FirstOrDefaultAsync(x => x.RouteId == routeId && x.SharedToId == shareToId);
            if (share == null)
                return NotFound("RouteShare not found");
            share.Status = RouteShare.AcceptanceStatus.Shared;
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

        public async Task<ServiceResult<List<RouteShare>>> GetSharesForUser(int userId)
        {
            var userCheck = await _context.Users.AsNoTracking()
                                                .FirstOrDefaultAsync(x => x.Id == userId);

            if (userCheck == null)
                return NotFound("User not found");

            return _context.RouteShares.AsNoTracking()
                .Include(x => x.Route)
                .ThenInclude(x => x.User)
                .Include(x => x.SharedTo)
                .Where(x => x.SharedToId == userId && x.Status == RouteShare.AcceptanceStatus.Shared)
                .ToList();
        }

        public async Task<ServiceResult<List<RouteShare>>> GetShareRequestsForUser(int userId)
        {
            var userCheck = await _context.Users.AsNoTracking()
                                                .FirstOrDefaultAsync(x => x.Id == userId);

            if (userCheck == null)
                return NotFound("User not found");

            return _context.RouteShares.AsNoTracking()
                .Include(x => x.Route)
                .ThenInclude(x => x.User)
                .Include(x => x.SharedTo)
                .Where(x => x.SharedToId == userId && x.Status == RouteShare.AcceptanceStatus.Sent)
                .ToList();
        }

        public async Task<RouteShare> GetRouteShare(int routeId, int userId)
        {
            var result = await _context.RouteShares.AsNoTracking()
                .Include(x => x.Route)
                .FirstOrDefaultAsync(x => x.RouteId == routeId && x.SharedToId == userId);

            return result;
        }
    }


}