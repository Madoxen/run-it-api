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
using Api.Tests.Mocks;
using Api.Payloads;
using System;
using System.Linq;
using Api.Services;
using Moq;
using Api.Utils;

namespace Api.Tests
{

    public class FriendControllerUnitTests
    {
        public FriendControllerUnitTests()
        { }

        private enum UserAuthorizationHandlerMode
        {
            SUCC = 0,
            FAIL = 1,
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

        private FriendsController CreateDefaultTestController(IFriendService friendService)
        {
            FriendsController controller = new FriendsController(friendService, ArrangeAuthService());
            controller.ControllerContext = ArrangeControllerContext();
            return controller;
        }


        [Fact]
        public async void TestGetEndpointWithExistingID()
        {

            //Arrange
            var friendServiceMock = new Mock<IFriendService>();
            friendServiceMock.Setup(x => x.GetFriends(1)).ReturnsAsync(new List<User>());
            FriendsController controller = CreateDefaultTestController(friendServiceMock.Object);

            //Act
            ActionResult<List<FriendPayload>> result = await controller.Get(1);

            //Assert
            Assert.IsType<List<FriendPayload>>(result.Value);
        }


        [Fact]
        public async void TestGetEndpointWithNonExistingID()
        {
            //Arrange
            var friendServiceMock = new Mock<IFriendService>();
            friendServiceMock.Setup(x => x.GetFriends(3)).ReturnsAsync(new NotFoundServiceResult("not found"));
            FriendsController controller = CreateDefaultTestController(friendServiceMock.Object);

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
            var friendServiceMock = new Mock<IFriendService>();
            friendServiceMock.Setup(x => x.RemoveFriend(1, 2)).ReturnsAsync(new SuccessServiceResult());
            FriendsController controller = CreateDefaultTestController(friendServiceMock.Object);

            //Act
            var result = await controller.Delete(1, 2);

            //Assert
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async void TestDeleteEndpointWithNonExistingID()
        {
            //Arrange
            var friendServiceMock = new Mock<IFriendService>();
            friendServiceMock.Setup(x => x.RemoveFriend(1, 2)).ReturnsAsync(new NotFoundServiceResult("blabla"));
            FriendsController controller = CreateDefaultTestController(friendServiceMock.Object);

            //Act
            var result = await controller.Delete(1, 2);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }


        [Fact]
        public async void TestGetUnauthorized()
        {
            //Arrange
            var friendServiceMock = new Mock<IFriendService>();
            friendServiceMock.Setup(x => x.GetFriends(1)).ReturnsAsync(new List<User>());
            FriendsController controller = new FriendsController(friendServiceMock.Object, ArrangeAuthService(handlerType: UserAuthorizationHandlerMode.FAIL));
            controller.ControllerContext = ArrangeControllerContext();
            //Act
            var result = await controller.Get(1);
            //Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }


        [Fact]
        public async void TestDeleteUnauthorized()
        {
            //Arrange
            var friendServiceMock = new Mock<IFriendService>();
            friendServiceMock.Setup(x => x.RemoveFriend(1, 2)).ReturnsAsync(new NotFoundServiceResult());
            FriendsController controller = new FriendsController(friendServiceMock.Object, ArrangeAuthService(handlerType: UserAuthorizationHandlerMode.FAIL));
            controller.ControllerContext = ArrangeControllerContext();
            //Act
            var result = await controller.Delete(1, 2);
            //Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

    }
}

