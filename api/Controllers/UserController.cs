using System.Threading.Tasks;
using Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Api.Models;
using Api.Handlers;
using Api.Payloads;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthorizationService _authorizationService;

        public UserController(IUserRepository userRepository, IAuthorizationService authorizationService)
        {
            _userRepository = userRepository;
            _authorizationService = authorizationService;
        }

        //Gets user profile information
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(int id)
        {
            User result = await _userRepository.Get(id);
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
            User result = await _userRepository.Get(id);
            if (result == null)
                return NotFound();

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, result, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                await _userRepository.Delete(id);
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
            User result = await _userRepository.Get(payload.Id);
            if (result == null)
                return NotFound();

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, result, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                User user = await _userRepository.Get(payload.Id);
                payload.ApplyToModel(user);
                await _userRepository.Update(user);
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
