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
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Api.Handlers;
using Api.Tests.Utils;
using Microsoft.Extensions.Options;
using Api.Configuration.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Api.Test.Mocks;

namespace Api.Tests
{

    public class UserControllerUnitTests
    {
        private enum UserAuthorizationHandlerMode
        {
            SUCC = 0,
            FAIL = 1,
        }

        private async Task<UserController> Arrange(
            IUserRepository repo = null,
            IAuthorizationService authService = null,
            List<Claim> claims = null,
            UserAuthorizationHandlerMode handlerType = UserAuthorizationHandlerMode.SUCC)
        {
            //Arrange user repository and add an user
            if (repo == null)
            {
                repo = new MockUserRepo();
                await repo.Add(new User() { Id = 1 });
            }


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


            //Arrange controller and supply it with context
            UserController controller = new UserController(repo, authService);
            controller.ControllerContext = context;

            return controller;
        }


        [Fact]
        public async void TestGetEndpointWithExistingID()
        {
            //Arrange
            UserController controller = await Arrange();

            //Act
            ActionResult<User> result = await controller.Get(1);

            //Assert
            Assert.IsType<User>(result.Value);
        }


        [Fact]
        public async void TestGetEndpointWithNonExistingID()
        {
            //Arrange
            UserController controller = await Arrange();

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
            var repo = new MockUserRepo();
            await repo.Add(new User() { Id = 1 });
            UserController controller = await Arrange(repo);

            //Act
            var result = await controller.Delete(1);
            var repo_result = await repo.Get(1);

            //Assert
            Assert.Null(repo_result);
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async void TestDeleteEndpointWithNonExistingID()
        {
            //Arrange
            var repo = new MockUserRepo();
            UserController controller = await Arrange(repo);


            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void TestUpdateEndpointWithExistingID()
        {
            //Arrange
            MockUserRepo repo = new MockUserRepo();
            UserController controller = await Arrange(repo);
            await repo.Add(new User() { Id = 1 });

            //Act
            await controller.Put(new Payloads.UserPayload()
            {
                Id = 1,
                Weight = 10
            });

            //Assert
            Assert.Equal(10, (await repo.Get(1)).Weight);
        }


        [Fact]
        public async void TestUpdateEndpointWithNonExistingID()
        {
            //Arrange
            MockUserRepo repo = new MockUserRepo();
            UserController controller = await Arrange(repo);
            await repo.Add(new User() { Id = 1, Weight = 1 });

            //Act
            var result = await controller.Put(new Payloads.UserPayload()
            {
                Id = 2,
                Weight = 10
            });

            //Assert
            Assert.IsType<NotFoundResult>(result);
            Assert.Equal(1, (await repo.Get(1)).Weight);
        }

        [Fact]
        public async void TestGetUnauthorized()
        {
            //Arrange
            UserController controller = await Arrange(handlerType: UserAuthorizationHandlerMode.FAIL);
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
            MockUserRepo repo = new MockUserRepo();
            await repo.Add(new User() { Id = 1, Weight = 1 });
            UserController controller = await Arrange(repo: repo, handlerType: UserAuthorizationHandlerMode.FAIL);
            //Act
            var result = await controller.Put(new Payloads.UserPayload()
            {
                Id = 1,
                Weight = 10
            });
            //Assert
            Assert.IsType<UnauthorizedResult>(result);
            Assert.Equal(1, (await repo.Get(1)).Weight);
        }

        [Fact]
        public async void TestDeleteUnauthorized()
        {
            //Arrange
            MockUserRepo repo = new MockUserRepo();
            await repo.Add(new User() { Id = 1, Weight = 1 });
            UserController controller = await Arrange(repo: repo, handlerType: UserAuthorizationHandlerMode.FAIL);
            //Act
            var result = await controller.Delete(1);
            //Assert
            Assert.IsType<UnauthorizedResult>(result);
            Assert.NotNull(await repo.Get(1));
        }

    }
}
