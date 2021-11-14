using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Api.Models;
using Api.Handlers;
using Api.Payloads;
using System.Linq;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly IAuthorizationService _authorizationService;

        public UserController(ApiContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        //Gets user profile information
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(int id)
        {
            User result = await _context.Users.FindAsync(id);
            if (result == null)
                return NotFound();

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, result, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            User target = await _context.Users.FindAsync(id);
            if (target == null)
                return NotFound("User not found");

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, target, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                _context.Users.Remove(target);
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }


        [HttpPut]
        public async Task<IActionResult> Put(UserPayload payload)
        {
            User user = await _context.Users.FindAsync(payload.Id);
            if (user == null)
                return NotFound();

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, user, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                payload.ApplyToModel(user);
                _context.Users.Update(user);
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
