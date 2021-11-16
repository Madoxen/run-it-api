using System.Threading.Tasks;
using Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Api.Models;
using Api.Handlers;
using Api.Payloads;
using System.Collections.Generic;
using System.Linq;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FriendRequestController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthorizationService _authorizationService;

        public FriendRequestController(IUserRepository userRepository, IAuthorizationService authorizationService)
        {
            _userRepository = userRepository;
            _authorizationService = authorizationService;
        }


        [HttpGet]
        [Route("{user_id}")]
        public async Task<ActionResult<List<FriendPayload>>> Get(int user_id)
        {
            User result = await _userRepository.Get(user_id);
            if (result == null)
                return NotFound("User not found");

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, result, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                if (result.FriendRequests == null)
                    return new List<FriendPayload>();
                return result.FriendRequests.Select(x => new FriendPayload(x)).ToList();
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        [Route("{user_id}/{friend_id}")]
        public async Task<IActionResult> Post(int user_id, int friend_id)
        {
            User user = await _userRepository.Get(user_id);
            User friend = await _userRepository.Get(friend_id);
            if (user == null)
                return NotFound("User not found");
            if (friend == null)
                return NotFound("Friend not found");



            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, user, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                if (user.FriendRequests.Exists(x => x.Id == friend_id))
                {
                    _userRepository.AddFriend(user_id, friend_id);
                }
                friend.FriendRequests.Add(user);
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }

        // [HttpDelete]
        // [Route("{user_id}/{friend_id}")]
        // public async Task<IActionResult> Delete(int user_id, int friend_id)
        // {

        // }
    }
}