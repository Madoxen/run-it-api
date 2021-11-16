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
    public class FriendsController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthorizationService _authorizationService;

        public FriendsController(IUserRepository userRepository, IAuthorizationService authorizationService)
        {
            _userRepository = userRepository;
            _authorizationService = authorizationService;
        }

        //Gets user profile information
        [HttpGet("{user_id}")]
        public async Task<ActionResult<List<FriendPayload>>> Get(int user_id)
        {
            User result = await _userRepository.Get(user_id);
            if (result == null)
                return NotFound();

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, result, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                return result.Friends.Select(x => new FriendPayload(x)).ToList();
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpDelete("{id}/{friend_id}")]
        public async Task<IActionResult> Delete(int id, int friend_id)
        {
            User user = await _userRepository.Get(id);
            User friend = await _userRepository.Get(friend_id);
            if (user == null)
                return NotFound("User not found");
            if (friend == null)
                return NotFound("Friend not found");


            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, user, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                _userRepository.DeleteFriend(id, friend_id);
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }

    }
}
