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
    public class FriendsController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly IAuthorizationService _authorizationService;

        public FriendsController(ApiContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        //Gets user profile information
        [HttpGet("{user_id}")]
        public async Task<ActionResult<List<FriendPayload>>> Get(int user_id)
        {
            User user = await _context.Users
            .Include(x => x.Friends)
            .FirstOrDefaultAsync(x => x.Id == user_id);

            if (user == null)
                return NotFound("User not found");

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, user, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                return user.Friends?.Select(x => new FriendPayload(x)).ToList();
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpDelete("{user_id}/{friend_id}")]
        public async Task<IActionResult> Delete(int user_id, int friend_id)
        {
            User user = await _context.Users
            .Include(x => x.Friends)
            .FirstOrDefaultAsync(x => x.Id == user_id);

            User friend = await _context.Users
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
                user.Friends.Remove(friend);
                friend.Friends.Remove(user);
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
