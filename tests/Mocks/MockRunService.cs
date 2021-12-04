using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Api.Models;
using Api.Services;
using Api.Utils;
using System;

namespace Api.Tests.Mocks
{
    public class MockRunService : ServiceBase, IRunService
    {

        public MockRunService(List<Run> runs, List<User> users)
        {
            usersStore = users;
            runsStore = runs;
        }

        public MockRunService()
        {
        }

        public List<Run> runsStore { get; set; } = new List<Run>();
        public List<User> usersStore { get; set; } = new List<User>();

        public async Task<Run> GetRunById(int id)
        {
            return runsStore.Find(x => x.Id == id);
        }

        public async Task<ServiceResult<List<Run>>> GetUserRuns(int userId)
        {
            var user = usersStore.Find(x => x.Id == userId);
            if (user == null)
                return NotFound("User not found");
            return user.Runs;
        }

        public async Task<ServiceResult> RemoveRunById(int id)
        {
            Run run = runsStore.Find(x => x.Id == id);
            if (run == null)
                return NotFound("User not found");
            runsStore.Remove(run);
            return Success();
        }

        public async Task<ServiceResult> RemoveRun(Run u)
        {
            runsStore.Remove(u);
            return Success();
        }

        public async Task<ServiceResult> UpdateRun(Run run)
        {
            int runIndex = runsStore.FindIndex(x => x.Id == run.Id);
            if (runIndex < 0)
                return NotFound("User not found");
            runsStore[runIndex] = run;
            return Success();
        }

        public async Task CreateRun(Run run)
        {
            runsStore.Add(run);
        }
    }
}
