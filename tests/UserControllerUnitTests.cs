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
using Api.Test.Mocks;
using Microsoft.EntityFrameworkCore;
using Api.Test.Fixtures;

namespace Api.Tests
{

    public class UserControllerUnitTests : IClassFixture<SharedDatabaseFixture>
    {
        public UserControllerUnitTests(SharedDatabaseFixture fixture)
        {
            DbFixture = fixture;
        }

        private enum UserAuthorizationHandlerMode
        {
            SUCC = 0,
            FAIL = 1,
        }

        public SharedDatabaseFixture DbFixture { get; }
        

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

        private UserController CreateDefaultTestController(ApiContext context)
        {
            UserController controller = new UserController(context, ArrangeAuthService());
            controller.ControllerContext = ArrangeControllerContext();
            return controller;
        }

        [Fact]
        public async void TestGetEndpointWithExistingID()
        {
            using (ApiContext context = DbFixture.CreateContext())
            {
                //Arrange
                UserController controller = CreateDefaultTestController(context);

                //Act
                ActionResult<User> result = await controller.Get(1);

                //Assert
                Assert.IsType<User>(result.Value);
            }
        }


        [Fact]
        public async void TestGetEndpointWithNonExistingID()
        {
            using (ApiContext context = DbFixture.CreateContext())
            {
                //Arrange
                UserController controller = CreateDefaultTestController(context);

                //Act
                ActionResult<User> result = await controller.Get(2);

                //Assert
                Assert.IsType<NotFoundResult>(result.Result);
                Assert.Null(result.Value);
            }
        }

        [Fact]
        public async void TestDeleteEndpointWithExistingID()
        {
            using (ApiContext context = DbFixture.CreateContext())
            {
                //Arrange
                UserController controller = CreateDefaultTestController(context);

                //Act
                var result = await controller.Delete(1);

                //Assert
                Assert.Null(context.Users.FindAsync(1));
                Assert.IsType<OkResult>(result);
            }
        }


        [Fact]
        public async void TestDeleteEndpointWithNonExistingID()
        {
            using (ApiContext context = DbFixture.CreateContext())
            {
                //Arrange
                UserController controller = CreateDefaultTestController(context);

                //Act
                var result = await controller.Delete(2);

                //Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void TestUpdateEndpointWithExistingID()
        {
            using (ApiContext context = DbFixture.CreateContext())
            {
                //Arrange
                UserController controller = CreateDefaultTestController(context);

                //Act
                await controller.Put(new Payloads.UserPayload()
                {
                    Id = 1,
                    Weight = 10
                });

                //Assert
                var result = await context.Users.FindAsync(1);
                Assert.Equal(10, result.Weight);
            }
        }


        [Fact]
        public async void TestUpdateEndpointWithNonExistingID()
        {
            using (ApiContext context = DbFixture.CreateContext())
            {
                //Arrange
                UserController controller = CreateDefaultTestController(context);

                //Act
                var result = await controller.Put(new Payloads.UserPayload()
                {
                    Id = 2,
                    Weight = 10
                });

                //Assert
                var context_result = await context.Users.FindAsync(1);
                Assert.IsType<NotFoundResult>(result);
                Assert.Equal(1, context_result.Weight);
            }
        }

        [Fact]
        public async void TestGetUnauthorized()
        {
            using (ApiContext context = DbFixture.CreateContext())
            {
                //Arrange
                UserController controller = CreateDefaultTestController(context);

                //Act
                var result = await controller.Get(1);

                //Assert
                Assert.IsType<UnauthorizedResult>(result.Result);
                Assert.Null(result.Value);
            }
        }

        [Fact]
        public async void TestUpdateUnauthorized()
        {
            using (ApiContext context = DbFixture.CreateContext())
            {
                //Arrange
                UserController controller = CreateDefaultTestController(context);

                //Act
                var result = await controller.Put(new Payloads.UserPayload()
                {
                    Id = 1,
                    Weight = 10
                });

                //Assert
                var context_result = await context.Users.FindAsync(1);
                Assert.IsType<UnauthorizedResult>(result);
                Assert.Equal(1, context_result.Weight);
            }
        }

        [Fact]
        public async void TestDeleteUnauthorized()
        {
            using (ApiContext context = DbFixture.CreateContext())
            {
                //Arrange
                UserController controller = CreateDefaultTestController(context);
                //Act
                var result = await controller.Delete(1);
                //Assert
                var context_result = await context.Users.FindAsync(1);
                Assert.IsType<UnauthorizedResult>(result);
                Assert.NotNull(context_result);
            }
        }

    }
}
