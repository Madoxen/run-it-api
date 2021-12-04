using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Api.Models;
using Api.Handlers;
using Api.Payloads;
using System.Linq;
using Api.Services;
using Api.Utils;
using System;
using System.Collections.Generic;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RunController : ControllerBase
    {
        private readonly IRunService _runService;
        private readonly IAuthorizationService _authorizationService;

        public RunController(
            IRunService runService,
            IAuthorizationService authorizationService)
        {
            _runService = runService;
            _authorizationService = authorizationService;
        }

        //Gets user profile information
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<RunGetPayload>>> GetUserRuns(int userId)
        {
            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, userId, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                var result = await _runService.GetUserRuns(userId);
                if (result.Value == null)
                    return (ActionResult)result.Result;
                var list = result.Value;
                return list.Select(x => new RunGetPayload(x)).ToList();
            }
            else
            {
                return Unauthorized();
            }
        }

        //Gets user profile information
        [HttpGet("{runId}")]
        public async Task<ActionResult<RunGetPayload>> GetRun(int runId)
        {
            Run targetRun = await _runService.GetRunById(runId);
            if (targetRun == null)
                return NotFound($"Run {runId} not found");

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, targetRun, "CheckRunUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                return new RunGetPayload(targetRun);
            }
            else
            {
                return Unauthorized();
            }
        }


        [HttpDelete("{runId}")]
        public async Task<ActionResult> Delete(int runId)
        {
            Run targetRun = await _runService.GetRunById(runId);
            if (targetRun == null)
                return NotFound($"Run {runId} not found");

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, targetRun, "CheckRunUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                var result = await _runService.RemoveRun(targetRun);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(RunCreatePayload payload)
        {
            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, payload.UserId, "CheckUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                Run run = payload.CreateModel();
                var result = await _runService.CreateRun(run);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPut]
        public async Task<ActionResult> Put(RunUpdatePayload payload)
        {
            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, payload.UserId, "CheckUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                Run run = payload.CreateModel();
                var result = await _runService.UpdateRun(run);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
