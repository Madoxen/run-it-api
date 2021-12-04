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
    public class RunControllerUnitTests
    {
        public RunControllerUnitTests()
        {
            List<Run> runs = new List<Run>();
            var run = new Run()
            {
                Id = 1,
                UserId = 1
            };
            runs.Add(run);

            List<User> users = new List<User>();
            var user = new User()
            {
                Id = 1,
                Weight = 1,
                Runs = runs
            };
            users.Add(user);

            _runService = new MockRunService(runs, users);
        }

        private IRunService _runService { get; set; }

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
                    {
                        services.AddSingleton<IAuthorizationHandler, MockUserAuthorizationHandlerSuccess>();
                        services.AddSingleton<IAuthorizationHandler, MockRunResourceAuthorizationHandlerSuccess>();
                    }


                    if (handlerType == UserAuthorizationHandlerMode.FAIL)
                    {
                        services.AddSingleton<IAuthorizationHandler, MockUserAuthorizationHandlerFail>();
                        services.AddSingleton<IAuthorizationHandler, MockRunResourceAuthorizationHandlerFail>();
                    }


                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy("CheckUserIDResourceAccess", policy => policy.Requirements.Add(new SameUserIDRequirement()));
                        options.AddPolicy("CheckRunUserIDResourceAccess", policy => policy.Requirements.Add(new RunUserOwnershipRequirement()));
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

        private RunController CreateDefaultTestController(IRunService runService, UserAuthorizationHandlerMode authMode = UserAuthorizationHandlerMode.SUCC)
        {
            RunController controller = new RunController(runService, ArrangeAuthService(handlerType: authMode));
            controller.ControllerContext = ArrangeControllerContext();
            return controller;
        }

        [Fact]
        public async void TestGetUserRunsEndpointWithExistingID()
        {

            //Arrange
            RunController controller = CreateDefaultTestController(_runService);

            //Act
            ActionResult<List<Run>> result = await controller.GetUserRuns(1);

            //Assert
            Assert.IsType<List<Run>>(result.Value);

        }


        [Fact]
        public async void TestGetUserRunsEndpointWithNonExistingID()
        {

            //Arrange
            RunController controller = CreateDefaultTestController(_runService);

            //Act
            ActionResult<List<Run>> result = await controller.GetUserRuns(2);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Null(result.Value);

        }


        [Fact]
        public async void TestGetRunEndpointWithExistingID()
        {

            //Arrange
            RunController controller = CreateDefaultTestController(_runService);

            //Act
            ActionResult<Run> result = await controller.GetRun(1);

            //Assert
            Assert.IsType<Run>(result.Value);
        }


        [Fact]
        public async void TestGetRunEndpointWithNonExistingID()
        {

            //Arrange
            RunController controller = CreateDefaultTestController(_runService);

            //Act
            ActionResult<Run> result = await controller.GetRun(2);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Null(result.Value);

        }



        [Fact]
        public async void TestDeleteEndpointWithExistingID()
        {
            //Arrange
            RunController controller = CreateDefaultTestController(_runService);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.IsType<OkResult>(result);
            Assert.Null(await _runService.GetRunById(1));
        }


        [Fact]
        public async void TestDeleteEndpointWithNonExistingID()
        {

            //Arrange
            RunController controller = CreateDefaultTestController(_runService);

            //Act
            var result = await controller.Delete(2);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);

        }

        [Fact]
        public async void TestUpdateEndpointWithExistingID()
        {

            //Arrange
            RunController controller = CreateDefaultTestController(_runService);

            //Act
            var result = await controller.Put(new Payloads.RunUpdatePayload()
            {
                Id = 1,
                Duration = 1
            });

            //Assert
            var result_service = await _runService.GetRunById(1);
            Assert.IsType<OkResult>(result);
            Assert.Equal(1u, result_service.Duration);
        }


        [Fact]
        public async void TestUpdateEndpointWithNonExistingID()
        {

            //Arrange
            RunController controller = CreateDefaultTestController(_runService);

            //Act
            var result = await controller.Put(new Payloads.RunUpdatePayload()
            {
                Id = 2,
                Duration = 1
            });

            //Assert
            var contextResult = await _runService.GetRunById(1);
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(0u, contextResult.Duration);
        }

        [Fact]
        public async void TestGetUserRunsUnauthorized()
        {
            //Arrange
            RunController controller = CreateDefaultTestController(_runService, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.GetUserRuns(1);

            //Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
            Assert.Null(result.Value);

        }

        [Fact]
        public async void TestGetUnauthorized()
        {
            //Arrange
            RunController controller = CreateDefaultTestController(_runService, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.GetRun(1);

            //Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
            Assert.Null(result.Value);

        }

        [Fact]
        public async void TestUpdateUnauthorized()
        {
            //Arrange
            RunController controller = CreateDefaultTestController(_runService, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.Put(new Payloads.RunUpdatePayload()
            {
                Id = 1,
                Duration = 1
            });

            //Assert
            var contextResult = await _runService.GetRunById(1);
            Assert.IsType<UnauthorizedResult>(result);
            Assert.Equal(0u, contextResult.Duration);
        }

        [Fact]
        public async void TestDeleteUnauthorized()
        {

            //Arrange
            RunController controller = CreateDefaultTestController(_runService, UserAuthorizationHandlerMode.FAIL);
            //Act
            var result = await controller.Delete(1);
            //Assert
            var contextResult = await _runService.GetRunById(1);
            Assert.IsType<UnauthorizedResult>(result);
            Assert.NotNull(contextResult);
        }
    }
}
