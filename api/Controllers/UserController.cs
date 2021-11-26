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

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;

        public UserController(IUserService userService, IAuthorizationService authorizationService)
        {
            _userService = userService;
            _authorizationService = authorizationService;
        }

        //Gets user profile information
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(int id)
        {
            User user = await _userService.GetUserById(id);
            if (user == null)
                return NotFound();

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, id, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                return user;
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, id, "CheckUserIDResourceAccess");

            if (authorizationResult.Succeeded)
            {
                var result = await _userService.RemoveUserById(id);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }


        [HttpPut]
        public async Task<ActionResult> Put(UserPayload payload)
        {

            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, payload.Id, "CheckUserIDResourceAccess");


            if (authorizationResult.Succeeded)
            {
                User user = payload.CreateModel();
                var result = await _userService.UpdateUser(user);
                return result;
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
