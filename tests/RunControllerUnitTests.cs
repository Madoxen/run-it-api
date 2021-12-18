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
using Api.Payloads;
using Moq;
using Api.Utils;

namespace Api.Tests
{
    public class RunControllerUnitTests
    {
        public RunControllerUnitTests()
        {
        }

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
            var runServiceMock = new Mock<IRunService>();
            runServiceMock.Setup(x => x.GetUserRuns(1, null)).ReturnsAsync(new List<Run>());
            RunController controller = CreateDefaultTestController(runServiceMock.Object);

            //Act
            ActionResult<List<RunGetPayload>> result = await controller.GetUserRuns(1);

            //Assert
            Assert.IsType<List<RunGetPayload>>(result.Value);
        }


        [Fact]
        public async void TestGetUserRunsEndpointWithNonExistingID()
        {
            //Arrange
            var runServiceMock = new Mock<IRunService>();
            runServiceMock.Setup(x => x.GetUserRuns(1, null)).ReturnsAsync(new NotFoundServiceResult("Not Found"));
            RunController controller = CreateDefaultTestController(runServiceMock.Object);

            //Act
            ActionResult<List<RunGetPayload>> result = await controller.GetUserRuns(1);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }


        [Fact]
        public async void TestGetRunEndpointWithExistingID()
        {
            //Arrange
            var runServiceMock = new Mock<IRunService>();
            runServiceMock.Setup(x => x.GetRunById(1)).ReturnsAsync(new Run());
            RunController controller = CreateDefaultTestController(runServiceMock.Object);

            //Act
            ActionResult<RunGetPayload> result = await controller.GetRun(1);

            //Assert
            Assert.IsType<RunGetPayload>(result.Value);
        }


        [Fact]
        public async void TestGetRunEndpointWithNonExistingID()
        {
            //Arrange
            var runServiceMock = new Mock<IRunService>();
            runServiceMock.Setup(x => x.GetRunById(1)).ReturnsAsync((Run)null);
            RunController controller = CreateDefaultTestController(runServiceMock.Object);

            //Act
            ActionResult<RunGetPayload> result = await controller.GetRun(1);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }



        [Fact]
        public async void TestDeleteEndpointWithExistingID()
        {
            //Arrange
            var run = new Run();
            var runServiceMock = new Mock<IRunService>();
            runServiceMock.Setup(x => x.GetRunById(1)).ReturnsAsync(run);
            runServiceMock.Setup(x => x.RemoveRun(run)).ReturnsAsync(new SuccessServiceResult());
            RunController controller = CreateDefaultTestController(runServiceMock.Object);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async void TestDeleteEndpointWithNonExistingID()
        {
            //Arrange
            var run = new Run();
            var runServiceMock = new Mock<IRunService>();
            runServiceMock.Setup(x => x.GetRunById(1)).ReturnsAsync((Run)null);
            RunController controller = CreateDefaultTestController(runServiceMock.Object);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async void TestUpdateEndpointWithExistingID()
        {
            //Arrange
            var run = new Run();
            var mockPayload = new Mock<RunUpdatePayload>();
            mockPayload.Setup(x => x.CreateModel()).Returns(run);

            var runService = new Mock<IRunService>();
            runService.Setup(x => x.GetRunById(1)).ReturnsAsync(run);
            runService.Setup(x => x.UpdateRun(run)).ReturnsAsync(new SuccessServiceResult());
            RunController controller = CreateDefaultTestController(runService.Object);

            //Act
            var result = await controller.Put(mockPayload.Object);

            //Assert
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async void TestUpdateEndpointWithNonExistingID()
        {
            //Arrange
            var run = new Run();
            var mockPayload = new Mock<RunUpdatePayload>();
            mockPayload.Setup(x => x.CreateModel()).Returns(run);

            var runService = new Mock<IRunService>();
            runService.Setup(x => x.GetRunById(1)).ReturnsAsync(run);
            runService.Setup(x => x.UpdateRun(run)).ReturnsAsync(new NotFoundServiceResult("Not found"));
            RunController controller = CreateDefaultTestController(runService.Object);

            //Act
            var result = await controller.Put(mockPayload.Object);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async void TestGetUserRunsUnauthorized()
        {
            //Arrange
            var runService = Mock.Of<IRunService>();
            RunController controller = CreateDefaultTestController(runService, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.GetUserRuns(1);

            //Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async void TestGetUnauthorized()
        {
            //Arrange
            var runServiceMock = new Mock<IRunService>();
            runServiceMock.Setup(x => x.GetRunById(1)).ReturnsAsync(new Run());
            RunController controller = CreateDefaultTestController(runServiceMock.Object, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.GetRun(1);

            //Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async void TestUpdateUnauthorized()
        {
            //Arrange
            var runService = Mock.Of<IRunService>();
            RunController controller = CreateDefaultTestController(runService, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.Put(new Payloads.RunUpdatePayload()
            {
                Id = 1,
                Duration = 1
            });

            //Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async void TestDeleteUnauthorized()
        {

            //Arrange
            var runServiceMock = new Mock<IRunService>();
            runServiceMock.Setup(x => x.GetRunById(1)).ReturnsAsync(new Run());
            RunController controller = CreateDefaultTestController(runServiceMock.Object, UserAuthorizationHandlerMode.FAIL);
            //Act
            var result = await controller.Delete(1);
            //Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
