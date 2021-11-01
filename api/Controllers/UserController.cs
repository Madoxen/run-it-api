using System.Threading.Tasks;
using Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        //Gets user profile information
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _userRepository.Get(id);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _userRepository.Delete(id);
            return Ok();
        }


    }
}
