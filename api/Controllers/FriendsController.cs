using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Api.Models;
using Api.Handlers;
using Api.Payloads;
using System.Collections.Generic;
using System.Linq;
using Api.Services;
using Api.Utils;
using System;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FriendsController : ControllerBase
    {
        private readonly IFriendService _friendService;
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;

        public FriendsController(IFriendService friendService, IUserService userService, IAuthorizationService authorizationService)
        {
            _friendService = friendService;
            _userService = userService;
            _authorizationService = authorizationService;
        }

        //Gets user profile information
        [HttpGet("{userId}")]
        public async Task<ActionResult<List<FriendPayload>>> Get(int userId)
        {
            var authorizationResult = await _authorizationService
            .AuthorizeAsync(User, userId, "CheckUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                var result = await _friendService.GetFriends(userId);
                if (result.Value == null)
                    return (ActionResult)result.Result;
                return result.Value.Select(x => new FriendPayload(x)).ToList();
            }
            else
            {
                return Unauthorized();
            }
        }

         //Gets user profile information
        [HttpGet("requests/{userId}")]
        public async Task<ActionResult<List<FriendPayload>>> GetFriendRequests(int userId)
        {
            var authorizationResult = await _authorizationService
            .AuthorizeAsync(User, userId, "CheckUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                var result = await _friendService.GetRequests(userId);
                if (result.Value == null)
                    return (ActionResult)result.Result;
                return result.Value.Select(x => new FriendPayload(x)).ToList();
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("{userId}/{friendId}")]
        public async Task<ActionResult> Post(int userId, int friendId)
        {

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, userId, "CheckUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                var result = await _friendService.SendFriendRequest(userId, friendId);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpDelete("{userId}/{friendId}")]
        public async Task<ActionResult> Delete(int userId, int friendId)
        {

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, userId, "CheckUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                var result = await _friendService.RemoveFriend(userId, friendId);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }

    }
}
