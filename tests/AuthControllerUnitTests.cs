using System.Collections.Generic;
using Api.Models;
using Xunit;
using Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Api.Configuration.Options;
using Api.Payloads;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Api.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using Api.Services;
using Api.Tests.Mocks;

namespace Api.Testss
{
    public class AuthControllerUnitTests
    {
        public AuthControllerUnitTests()
        {
            User u = new User()
            {
                Id = 1
            };
            List<User> users = new List<User>();
            users.Add(u);
            _userService = new MockUserService(users);
            _userAuthService = new MockUserAuthService(users);
        }

        private IUserService _userService { get; set; }
        private IUserAuthService _userAuthService { get; set; }

        private enum UserAuthorizationHandlerMode
        {
            SUCC = 0,
            FAIL = 1,
        }


        [Fact]
        public async void TestGetRefreshTokenEndpointValidToken()
        {
            //Arrange

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
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user,
                }
            };

            AuthController controller = new AuthController(_userAuthService, _userService, null, options);
            controller.ControllerContext = controllerContext;

            //Act
            ActionResult<JWTAuthPayload> result = await controller.GetRefereshToken();

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Value);

        }
    }
}
