using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Api.Models;
using Api.Repositories;
using Xunit;
using Api.Controllers;
using Api.Tests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Api.Configuration.Options;
using Api.Payloads;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;


namespace Api.Tests
{
    public class AuthControllerUnitTests
    {
        [Fact]
        public async void TestGetRefreshTokenEndpointValidToken()
        {
            //Arrange
            MockUserRepo repo = new MockUserRepo();
            IOptions<AuthenticationOptions> options = Options.Create(new AuthenticationOptions()
            {
                JWT = new JWTOptions()
                {
                    Audience = "runit.co",
                    Issuer = "runit.co",
                    ExpiryMinutes = 20,
                    Key = "aaabbbcccdddeeefffggghhhiiijjjkkk"
                }
            });

            var claims = new List<Claim>()
            {
                new Claim("sub", "1"),
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user,
                }
            };

            AuthController controller = new AuthController(repo, null, options);
            controller.ControllerContext = context;
            await repo.Add(new User() { Id = 1 });

            //Act
            ActionResult<JWTAuthPayload> result = await controller.GetRefereshToken();

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
        }
    }
}
