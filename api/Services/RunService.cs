using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;
using Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IRunService
    {
        Task<Run> GetRunById(int id);
        Task<ServiceResult<List<Run>>> GetUserRuns(int userId);
        Task<ServiceResult> RemoveRunById(int id);
        Task<ServiceResult> RemoveRun(Run u);
        Task<ServiceResult> UpdateRun(Run u);
        Task<ServiceResult> CreateRun(Run u);
    }

    public class RunService : ServiceBase, IRunService
    {
        public readonly ApiContext _context;
        public RunService(ApiContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> CreateRun(Run run)
        {
            _context.Runs.Add(run);
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<Run> GetRunById(int id)
        {
            var result = await _context.Runs.FindAsync(id);
            return result;
        }

        public async Task<ServiceResult<List<Run>>> GetUserRuns(int userId)
        {
            var users = _context.Users
            .Include(x => x.Runs);

            var user = await users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return NotFound("User not found");
            return user.Runs;
        }

        public async Task<ServiceResult> RemoveRun(Run u)
        {
            _context.Runs.Remove(u);
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<ServiceResult> RemoveRunById(int id)
        {
            Run run = await _context.Runs.FindAsync(id);
            if (run == null)
                return NotFound("User not found");
            _context.Runs.Remove(run);
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<ServiceResult> UpdateRun(Run u)
        {
            Run user = await _context.Runs.FindAsync(u.Id);
            if (user == null)
                return NotFound("User not found");
            _context.Runs.Update(user);
            await _context.SaveChangesAsync();
            return Success();
        }
    }

}