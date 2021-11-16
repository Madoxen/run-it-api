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

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FriendRequestController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ApiContext _context;

        public FriendRequestController(ApiContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }


        [HttpGet]
        [Route("{user_id}")]
        public async Task<ActionResult<List<FriendPayload>>> Get(int user_id)
        {
            User user = await _context.Users
            .Include(x => x.FriendRequests)
            .FirstOrDefaultAsync(x => x.Id == user_id);

            if (user == null)
                return NotFound("User not found");

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, user, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                if (user.FriendRequests == null)
                    return new List<FriendPayload>();
                return user.FriendRequests?.Select(x => new FriendPayload(x)).ToList();
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
            User user = await _context.Users
           .Include(x => x.FriendRequests)
           .Include(x => x.Friends)
           .FirstOrDefaultAsync(x => x.Id == user_id);

            User friend = await _context.Users
            .Include(x => x.FriendRequests)
            .Include(x => x.Friends)
            .FirstOrDefaultAsync(x => x.Id == friend_id);

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
                    user.Friends.Add(friend);
                    friend.Friends.Add(user);
                }

                friend.FriendRequests.Add(user);
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpDelete]
        [Route("{user_id}/{friend_id}")]
        public async Task<IActionResult> Delete(int user_id, int friend_id)
        {
            User user = await _context.Users
            .Include(x => x.FriendRequests)
            .FirstOrDefaultAsync(x => x.Id == user_id);

            User friend = await _context.Users
            .Include(x => x.FriendRequests)
            .FirstOrDefaultAsync(x => x.Id == friend_id);

            if (user == null)
                return NotFound("User not found");
            if (friend == null)
                return NotFound("Friend not found");

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, user, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                user.FriendRequests.Remove(friend);
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}