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
    public class RouteShareController : ControllerBase
    {
        private readonly IRouteShareService _routeShareService;
        private readonly IAuthorizationService _authorizationService;

        public RouteShareController(
            IRouteShareService routeShareService,
            IAuthorizationService authorizationService)
        {
            _routeShareService = routeShareService;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        [Route("{routeId}/{shareId}")]
        public async Task<ActionResult> Post(int routeId, int shareId)
        {
            RouteShare targetShare = await _routeShareService.GetRouteShare(routeId, shareId);
            if (targetShare == null)
                return NotFound($"Route share {routeId} to {shareId} not found");

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, targetShare, "CheckRouteShareUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                var result = await _routeShareService.ShareRouteWith(routeId, shareId);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpDelete]
        [Route("{routeId}/{shareId}")]
        public async Task<ActionResult> Delete(int routeId, int shareId)
        {
            RouteShare targetShare = await _routeShareService.GetRouteShare(routeId, shareId);
            if (targetShare == null)
                return NotFound($"Route share {routeId} to {shareId} not found");

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, targetShare, "CheckRouteShareUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                var result = await _routeShareService.RemoveShare(routeId, shareId);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<ActionResult<List<RouteShareGetPayload>>> Get(int userId)
        {
            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, userId, "CheckUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                var result = await _routeShareService.GetSharesForUser(userId);
                if (result.Value == null)
                    return (ActionResult)result.Result;
                var list = result.Value;
                return list.Select(x => new RouteShareGetPayload(x)).ToList();
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
