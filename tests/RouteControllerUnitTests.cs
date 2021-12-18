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
    public class RouteControllerUnitTests
    {
        public RouteControllerUnitTests()
        {
            List<Route> routes = new List<Route>();
            var route = new Route()
            {
                Id = 1,
                UserId = 1,
                Title = "xd"
            };
            routes.Add(route);

            List<User> users = new List<User>();
            var user = new User()
            {
                Id = 1,
                Weight = 1,
                Routes = routes
            };
            users.Add(user);
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
                    Audience = "routeit.co",
                    Issuer = "routeit.co",
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
                        services.AddSingleton<IAuthorizationHandler, MockRouteResourceAuthorizationHandlerSuccess>();
                    }


                    if (handlerType == UserAuthorizationHandlerMode.FAIL)
                    {
                        services.AddSingleton<IAuthorizationHandler, MockUserAuthorizationHandlerFail>();
                        services.AddSingleton<IAuthorizationHandler, MockRouteResourceAuthorizationHandlerFail>();
                    }


                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy("CheckUserIDResourceAccess", policy => policy.Requirements.Add(new SameUserIDRequirement()));
                        options.AddPolicy("CheckRouteUserIDResourceAccess", policy => policy.Requirements.Add(new RouteUserOwnershipRequirement()));
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

        private RouteController CreateDefaultTestController(IRouteService routeService, UserAuthorizationHandlerMode authMode = UserAuthorizationHandlerMode.SUCC)
        {
            RouteController controller = new RouteController(routeService, ArrangeAuthService(handlerType: authMode));
            controller.ControllerContext = ArrangeControllerContext();
            return controller;
        }

        [Fact]
        public async void TestGetUserRoutesEndpointWithExistingID()
        {
            //Arrange
            var routeService = new Mock<IRouteService>();
            routeService.Setup(x => x.GetUserRoutes(1)).ReturnsAsync(new List<Route>());
            RouteController controller = CreateDefaultTestController(routeService.Object);

            //Act
            ActionResult<List<RouteGetPayload>> result = await controller.GetUserRoutes(1);

            //Assert
            Assert.IsType<List<RouteGetPayload>>(result.Value);
        }


        [Fact]
        public async void TestGetUserRoutesEndpointWithNonExistingID()
        {
            //Arrange
            var routeService = new Mock<IRouteService>();
            routeService.Setup(x => x.GetUserRoutes(1)).ReturnsAsync(new NotFoundServiceResult("Not found"));
            RouteController controller = CreateDefaultTestController(routeService.Object);

            //Act
            ActionResult<List<RouteGetPayload>> result = await controller.GetUserRoutes(1);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Null(result.Value);

        }


        [Fact]
        public async void TestGetRouteEndpointWithExistingID()
        {
            //Arrange
            var routeService = new Mock<IRouteService>();
            routeService.Setup(x => x.GetRouteById(1)).ReturnsAsync(new Route() { Id = 1 });
            RouteController controller = CreateDefaultTestController(routeService.Object);

            //Act
            ActionResult<RouteGetPayload> result = await controller.GetRoute(1);

            //Assert
            Assert.IsType<RouteGetPayload>(result.Value);
        }


        [Fact]
        public async void TestGetRouteEndpointWithNonExistingID()
        {
            //Arrange
            var routeService = new Mock<IRouteService>();
            routeService.Setup(x => x.GetRouteById(2)).ReturnsAsync((Route)null);
            RouteController controller = CreateDefaultTestController(routeService.Object);

            //ActS
            ActionResult<RouteGetPayload> result = await controller.GetRoute(1);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }



        [Fact]
        public async void TestDeleteEndpointWithExistingID()
        {
            //Arrange
            var route = new Route() { Id = 1 };
            var routeService = new Mock<IRouteService>();
            routeService.Setup(x => x.GetRouteById(1)).ReturnsAsync(route);
            routeService.Setup(x => x.RemoveRoute(route)).ReturnsAsync(new SuccessServiceResult());
            RouteController controller = CreateDefaultTestController(routeService.Object);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async void TestDeleteEndpointWithNonExistingID()
        {
            //Arrange
            var routeService = new Mock<IRouteService>();
            routeService.Setup(x => x.GetRouteById(1)).ReturnsAsync((Route)null);
            RouteController controller = CreateDefaultTestController(routeService.Object);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);

        }

        [Fact]
        public async void TestUpdateEndpointWithExistingID()
        {

            //Arrange
            var route = new Route();
            var mockPayload = new Mock<RouteUpdatePayload>();
            mockPayload.Setup(x => x.CreateModel()).Returns(route);

            var routeService = new Mock<IRouteService>();
            routeService.Setup(x => x.UpdateRoute(route)).ReturnsAsync(new SuccessServiceResult());
            
            RouteController controller = CreateDefaultTestController(routeService.Object);

            //Act
            var result = await controller.Put(mockPayload.Object);

            //Assert
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async void TestUpdateEndpointWithNonExistingID()
        {
            //Arrange
            var route = new Route();
            var mockPayload = new Mock<RouteUpdatePayload>();
            mockPayload.Setup(x => x.CreateModel()).Returns(route);

            var routeService = new Mock<IRouteService>();
            routeService.Setup(x => x.UpdateRoute(route)).ReturnsAsync(new NotFoundServiceResult("Not found"));
            RouteController controller = CreateDefaultTestController(routeService.Object);

            //Act
            var result = await controller.Put(mockPayload.Object);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async void TestGetUserRoutesUnauthorized()
        {
            //Arrange
            var routeService = new Mock<IRouteService>();
            RouteController controller = CreateDefaultTestController(routeService.Object, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.GetUserRoutes(1);

            //Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
            Assert.Null(result.Value);

        }

        [Fact]
        public async void TestGetUnauthorized()
        {
            //Arrange
            var routeService = new Mock<IRouteService>();
            routeService.Setup(x => x.GetRouteById(1)).ReturnsAsync(new Route());
            RouteController controller = CreateDefaultTestController(routeService.Object, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.GetRoute(1);

            //Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async void TestUpdateUnauthorized()
        {
            //Arrange
            var routeService = new Mock<IRouteService>();
            RouteController controller = CreateDefaultTestController(routeService.Object, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.Put(new Payloads.RouteUpdatePayload()
            {
                Id = 1,
                Title = "xd"
            });

            //Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async void TestDeleteUnauthorized()
        {

            //Arrange
            var routeService = new Mock<IRouteService>();
            routeService.Setup(x => x.GetRouteById(1)).ReturnsAsync(new Route());
            RouteController controller = CreateDefaultTestController(routeService.Object, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
