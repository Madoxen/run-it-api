using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Api.Models;
using Xunit;
using Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Api.Handlers;
using Api.Tests.Utils;
using Microsoft.Extensions.Options;
using Api.Configuration.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Api.Test.Mocks;
using Api.Payloads;
using System;
using System.Linq;

namespace Api.Tests
{

    public class FriendControllerUnitTests : IDisposable
    {
        public FriendControllerUnitTests()
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
            ApiContext.Database.EnsureCreated();

            var user = new User()
            {
                Id = 1,
                Weight = 1,
            };

            var friend = new User()
            {
                Id = 2,
                Weight = 1,
            };

            ApiContext.Users.Add(user);
            ApiContext.Users.Add(friend);
            user.Friends = new List<User>();
            friend.Friends = new List<User>();
            user.Friends.Add(friend);
            friend.Friends.Add(user);
            ApiContext.SaveChanges();
        }

        private IAuthorizationService ArrangeAuthService(
            IAuthorizationService authService = null,
            UserAuthorizationHandlerMode handlerType = UserAuthorizationHandlerMode.SUCC)
        {

            //Arrange Authentication context
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

            if (authService == null)
            {
                //Create authorization service
                authService = Builders.BuildAuthorizationService(services =>
                {
                    if (handlerType == UserAuthorizationHandlerMode.SUCC)
                        services.AddScoped<IAuthorizationHandler, MockUserAuthorizationHandlerSuccess>();

                    if (handlerType == UserAuthorizationHandlerMode.FAIL)
                        services.AddScoped<IAuthorizationHandler, MockUserAuthorizationHandlerFail>();

                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy("CheckUserIDResourceAccess", policy => policy.Requirements.Add(new SameUserIDRequirement()));
                    });
                });
            }

            return authService;
        }

        private ControllerContext ArrangeControllerContext(List<Claim> claims = null)
        {
            if (claims == null)
            {
                claims = new List<Claim>()
                {
                    new Claim("sub", "1"),
                };
            }

            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            //Create controller context and supply it with security details
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user,
                }
            };

            return context;
        }

        private FriendsController CreateDefaultTestController()
        {
            FriendsController controller = new FriendsController(ApiContext, ArrangeAuthService());
            controller.ControllerContext = ArrangeControllerContext();
            return controller;
        }


        [Fact]
        public async void TestGetEndpointWithExistingID()
        {
            //Arrange
            FriendsController controller = CreateDefaultTestController();

            //Act
            ActionResult<List<FriendPayload>> result = await controller.Get(1);

            //Assert
            Assert.IsType<List<FriendPayload>>(result.Value);
        }


        [Fact]
        public async void TestGetEndpointWithNonExistingID()
        {
            //Arrange
            FriendsController controller = CreateDefaultTestController();

            //Act
            ActionResult<List<FriendPayload>> result = await controller.Get(3);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Null(result.Value);
        }


        [Fact]
        public async void TestDeleteEndpointWithExistingID()
        {
            //Arrange
            FriendsController controller = CreateDefaultTestController();

            //Act
            var result = await controller.Delete(1, 2);
            var context_result = ApiContext.Users.Include(x => x.Friends).First(x => x.Id == 1);

            //Assert
            Assert.Empty(context_result.Friends);
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async void TestDeleteEndpointWithNonExistingID()
        {
            //Arrange
            FriendsController controller = CreateDefaultTestController();


            //Act
            var result = await controller.Delete(1, 3);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }


        [Fact]
        public async void TestGetUnauthorized()
        {
            //Arrange
            FriendsController controller = new FriendsController(ApiContext, ArrangeAuthService(handlerType: UserAuthorizationHandlerMode.FAIL));
            controller.ControllerContext = ArrangeControllerContext();
            //Act
            var result = await controller.Get(1);
            //Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
            Assert.Null(result.Value);
        }


        [Fact]
        public async void TestDeleteUnauthorized()
        {
            //Arrange
            FriendsController controller = new FriendsController(ApiContext, ArrangeAuthService(handlerType: UserAuthorizationHandlerMode.FAIL));
            controller.ControllerContext = ArrangeControllerContext();
            //Act
            var result = await controller.Delete(1, 2);
            var context_result = await ApiContext.Users.Include(x => x.Friends).FirstAsync(x => x.Id == 1);
            //Assert
            Assert.IsType<UnauthorizedResult>(result);

            Assert.Equal(1, context_result.Friends.Count());
        }

        public void Dispose()
        {
            ApiContext.Dispose();
        }
    }
}

