using System.Collections.Generic;
using System.Threading.Tasks;
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
using Microsoft.EntityFrameworkCore;
using System;
using Api.Services;

namespace Api.Tests
{
    public class UserControllerUnitTests
    {
        public UserControllerUnitTests()
        {
            List<User> users = new List<User>();
            var user = new User()
            {
                Id = 1,
                Weight = 1,
            };
            users.Add(user);

            _userService = new MockUserService(users);
        }

        private IUserService _userService { get; set; }

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

        private UserController CreateDefaultTestController(IUserService userService, UserAuthorizationHandlerMode authMode = UserAuthorizationHandlerMode.SUCC)
        {
            UserController controller = new UserController(userService, ArrangeAuthService(handlerType: authMode));
            controller.ControllerContext = ArrangeControllerContext();
            return controller;
        }

        [Fact]
        public async void TestGetEndpointWithExistingID()
        {

            //Arrange
            UserController controller = CreateDefaultTestController(_userService);

            //Act
            ActionResult<User> result = await controller.Get(1);

            //Assert
            Assert.IsType<User>(result.Value);

        }


        [Fact]
        public async void TestGetEndpointWithNonExistingID()
        {

            //Arrange
            UserController controller = CreateDefaultTestController(_userService);

            //Act
            ActionResult<User> result = await controller.Get(2);

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
            Assert.Null(result.Value);

        }

        [Fact]
        public async void TestDeleteEndpointWithExistingID()
        {
            //Arrange
            UserController controller = CreateDefaultTestController(_userService);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.Null(await _userService.GetUserById(1));
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async void TestDeleteEndpointWithNonExistingID()
        {

            //Arrange
            UserController controller = CreateDefaultTestController(_userService);

            //Act
            var result = await controller.Delete(2);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);

        }

        [Fact]
        public async void TestUpdateEndpointWithExistingID()
        {

            //Arrange
            UserController controller = CreateDefaultTestController(_userService);

            //Act
            var result = await controller.Put(new Payloads.UserPayload()
            {
                Id = 1,
                Weight = 10
            });

            //Assert
            var result_service = await _userService.GetUserById(1);
            Assert.IsType<OkResult>(result);
            Assert.Equal(10, result_service.Weight);
        }


        [Fact]
        public async void TestUpdateEndpointWithNonExistingID()
        {

            //Arrange
            UserController controller = CreateDefaultTestController(_userService);

            //Act
            var result = await controller.Put(new Payloads.UserPayload()
            {
                Id = 2,
                Weight = 10
            });

            //Assert
            var contextResult = await _userService.GetUserById(1);
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(1, contextResult.Weight);
        }

        [Fact]
        public async void TestGetUnauthorized()
        {
            //Arrange
            UserController controller = CreateDefaultTestController(_userService, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
            Assert.Null(result.Value);

        }

        [Fact]
        public async void TestUpdateUnauthorized()
        {
            //Arrange
            UserController controller = CreateDefaultTestController(_userService, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.Put(new Payloads.UserPayload()
            {
                Id = 1,
                Weight = 10
            });

            //Assert
            var contextResult = await _userService.GetUserById(1);
            Assert.IsType<UnauthorizedResult>(result);
            Assert.Equal(1, contextResult.Weight);
        }

        [Fact]
        public async void TestDeleteUnauthorized()
        {

            //Arrange
            UserController controller = CreateDefaultTestController(_userService, UserAuthorizationHandlerMode.FAIL);
            //Act
            var result = await controller.Delete(1);
            //Assert
            var contextResult = await _userService.GetUserById(1);
            Assert.IsType<UnauthorizedResult>(result);
            Assert.NotNull(contextResult);
        }
    }
}
