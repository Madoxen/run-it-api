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
        [HttpGet("/user/{userId}")]
        public async Task<ActionResult<List<Run>>> Get(int userId)
        {
            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, userId, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                return await _runService.GetUserRuns(userId);
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, id, "CheckUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                var result = await _runService.RemoveRunById(id);
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
                    .AuthorizeAsync(User, payload.Id, "CheckUserIDResourceAccess");


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
