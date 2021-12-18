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
    public class RouteController : ControllerBase
    {
        private readonly IRouteService _routeService;
        private readonly IAuthorizationService _authorizationService;

        public RouteController(
            IRouteService routeService,
            IAuthorizationService authorizationService)
        {
            _routeService = routeService;
            _authorizationService = authorizationService;
        }

        //Gets user profile information
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<RouteGetPayload>>> GetUserRoutes(int userId)
        {
            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, userId, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                var result = await _routeService.GetUserRoutes(userId);
                if (result.Value == null)
                    return (ActionResult)result.Result;
                var list = result.Value;
                return list.Select(x => new RouteGetPayload(x)).ToList();
            }
            else
            {
                return Unauthorized();
            }
        }

        //Gets user profile information
        [HttpGet("{routeId}")]
        public async Task<ActionResult<RouteGetPayload>> GetRoute(int routeId)
        {
            Route targetRoute = await _routeService.GetRouteById(routeId);
            if (targetRoute == null)
                return NotFound($"Route {routeId} not found");

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, targetRoute, "CheckRouteUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                return new RouteGetPayload(targetRoute);
            }
            else
            {
                return Unauthorized();
            }
        }


        [HttpDelete("{routeId}")]
        public async Task<ActionResult> Delete(int routeId)
        {
            Route targetRoute = await _routeService.GetRouteById(routeId);
            if (targetRoute == null)
                return NotFound($"Route {routeId} not found");

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, targetRoute, "CheckRouteUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                var result = await _routeService.RemoveRoute(targetRoute);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(RouteCreatePayload payload)
        {
            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, payload.UserId, "CheckUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                Route route = payload.CreateModel();
                var result = await _routeService.CreateRoute(route);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPut]
        public async Task<ActionResult> Put(RouteUpdatePayload payload)
        {
            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, payload.UserId, "CheckUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                Route route = payload.CreateModel();
                var result = await _routeService.UpdateRoute(route);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
