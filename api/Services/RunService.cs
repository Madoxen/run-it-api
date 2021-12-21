using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;
using Api.Utils;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IRunService
    {
        Task<Run> GetRunById(int id);
        Task<ServiceResult<List<Run>>> GetUserRuns(int userId, DateTimeOffset? from = null);
        Task<ServiceResult> RemoveRunById(int id);
        Task<ServiceResult> RemoveRun(Run u);
        Task<ServiceResult> UpdateRun(Run u);
        Task<ServiceResult<Run>> CreateRun(Run u);
    }

    public class RunService : ServiceBase, IRunService
    {
        public readonly ApiContext _context;
        public RunService(ApiContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<Run>> CreateRun(Run run)
        {
            if (await _context.Users.FirstOrDefaultAsync(x => x.Id == run.UserId) == null)
                return NotFound("Cannot create run for non existing user");
            await _context.Runs.AddAsync(run);
            await _context.SaveChangesAsync();
            return run;
        }

        public async Task<Run> GetRunById(int id)
        {
            var result = await _context.Runs.FindAsync(id);
            return result;
        }

        public async Task<ServiceResult<List<Run>>> GetUserRuns(int userId, DateTimeOffset? from = null)
        {
            var users = _context.Users
            .Include(x => x.Runs);

            var user = await users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return NotFound("User not found");
            if (from == null)
                return user.Runs;
            return user.Runs.FindAll(x => x.Date >= from);
        }

        public async Task<ServiceResult> RemoveRun(Run run)
        {
            var runQueryResult = await _context.Runs.FirstOrDefaultAsync(x => x.Id == run.Id);
            if (runQueryResult == null)
                return NotFound("Run not found");

            _context.Runs.Remove(runQueryResult);
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<ServiceResult> RemoveRunById(int id)
        {
            Run run = await _context.Runs.FindAsync(id);
            if (run == null)
                return NotFound("Run not found");

            _context.Runs.Remove(run);
            await _context.SaveChangesAsync();
            return Success();
        }

        public async Task<ServiceResult> UpdateRun(Run run)
        {
            var check = _context.Runs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == run.Id);

            var userCheck = _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == run.UserId);

            if (await check == null)
                return NotFound("Run not found");
            if (await userCheck == null)
                return NotFound("Cannot update run with non existing user as a owner");

            _context.Runs.Update(run);
            await _context.SaveChangesAsync();
            return Success();
        }
    }

}