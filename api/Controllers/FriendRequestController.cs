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

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FriendRequestController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IFriendRequestService _friendRequestService;

        public FriendRequestController(IFriendRequestService friendRequestService, IUserService userService, IAuthorizationService authorizationService)
        {
            _friendRequestService = friendRequestService;
            _authorizationService = authorizationService;
            _userService = userService;
        }


        [HttpGet]
        [Route("{userId}")]
        public async Task<ActionResult<List<FriendPayload>>> Get(int userId)
        {
            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, userId, "CheckUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                var result = await _friendRequestService.GetFriendRequests(userId);
                if (result.Value == null)
                    return (ActionResult)(result.Result);
                return result.Value.Select(x => new FriendPayload(x)).ToList();
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        [Route("{userId}/{friendId}")]
        public async Task<ActionResult> Post(int userId, int friendId)
        {
            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, userId, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                var result = await _friendRequestService.AddFriendRequest(userId, friendId);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpDelete]
        [Route("{userId}/{friendId}")]
        public async Task<ActionResult> Delete(int userId, int friendId)
        {

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, userId, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                var result = await _friendRequestService.RemoveFriendRequest(userId, friendId);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}