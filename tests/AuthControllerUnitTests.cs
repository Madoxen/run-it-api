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

namespace Api.Testss
{
    public class AuthControllerUnitTests : IDisposable
    {
        public AuthControllerUnitTests()
        {
            var options = new DbContextOptionsBuilder<ApiContext>()
              .UseNpgsql($"Server=test_db;Database={Guid.NewGuid().ToString()};Username=admin;Password=admin")
              .Options;

            ApiContext = new ApiContext(options);

            //insert the data that you want to be seeded for each test method:
            Seed();
        }

        private ApiContext ApiContext { get; set; }

        private enum UserAuthorizationHandlerMode
        {
            SUCC = 0,
            FAIL = 1,
        }

        private void Seed()
        {
            ApiContext.Database.EnsureDeleted();
            ApiContext.Database.EnsureCreated();

            var user = new User()
            {
                Id = 1,
            };

            ApiContext.Users.Add(user);
            ApiContext.SaveChanges();

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

            AuthController controller = new AuthController(ApiContext, null, options);
            controller.ControllerContext = controllerContext;

            //Act
            ActionResult<JWTAuthPayload> result = await controller.GetRefereshToken();

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Value);

        }

        public void Dispose()
        {
            ApiContext.Dispose();
        }

    }
}
