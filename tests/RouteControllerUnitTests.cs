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

            _routeService = new MockRouteService(routes, users);
        }

        private IRouteService _routeService { get; set; }

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
            RouteController controller = CreateDefaultTestController(_routeService);

            //Act
            ActionResult<List<RouteGetPayload>> result = await controller.GetUserRoutes(1);

            //Assert
            Assert.IsType<List<RouteGetPayload>>(result.Value);

        }


        [Fact]
        public async void TestGetUserRoutesEndpointWithNonExistingID()
        {

            //Arrange
            RouteController controller = CreateDefaultTestController(_routeService);

            //Act
            ActionResult<List<RouteGetPayload>> result = await controller.GetUserRoutes(2);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Null(result.Value);

        }


        [Fact]
        public async void TestGetRouteEndpointWithExistingID()
        {

            //Arrange
            RouteController controller = CreateDefaultTestController(_routeService);

            //Act
            ActionResult<RouteGetPayload> result = await controller.GetRoute(1);

            //Assert
            Assert.IsType<RouteGetPayload>(result.Value);
        }


        [Fact]
        public async void TestGetRouteEndpointWithNonExistingID()
        {

            //Arrange
            RouteController controller = CreateDefaultTestController(_routeService);

            //Act
            ActionResult<RouteGetPayload> result = await controller.GetRoute(2);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Null(result.Value);

        }



        [Fact]
        public async void TestDeleteEndpointWithExistingID()
        {
            //Arrange
            RouteController controller = CreateDefaultTestController(_routeService);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.IsType<OkResult>(result);
            Assert.Null(await _routeService.GetRouteById(1));
        }


        [Fact]
        public async void TestDeleteEndpointWithNonExistingID()
        {

            //Arrange
            RouteController controller = CreateDefaultTestController(_routeService);

            //Act
            var result = await controller.Delete(2);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);

        }

        [Fact]
        public async void TestUpdateEndpointWithExistingID()
        {

            //Arrange
            RouteController controller = CreateDefaultTestController(_routeService);

            //Act
            var result = await controller.Put(new Payloads.RouteUpdatePayload()
            {
                Id = 1,
                Title = "haha"
            });

            //Assert
            var result_service = await _routeService.GetRouteById(1);
            Assert.IsType<OkResult>(result);
            Assert.Equal("haha", result_service.Title);
        }


        [Fact]
        public async void TestUpdateEndpointWithNonExistingID()
        {
            //Arrange
            RouteController controller = CreateDefaultTestController(_routeService);

            //Act
            var result = await controller.Put(new Payloads.RouteUpdatePayload()
            {
                Id = 2,
                Title = "xd"
            });

            //Assert
            var contextResult = await _routeService.GetRouteById(1);
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("xd", contextResult.Title);
        }

        [Fact]
        public async void TestGetUserRoutesUnauthorized()
        {
            //Arrange
            RouteController controller = CreateDefaultTestController(_routeService, UserAuthorizationHandlerMode.FAIL);

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
            RouteController controller = CreateDefaultTestController(_routeService, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.GetRoute(1);

            //Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
            Assert.Null(result.Value);

        }

        [Fact]
        public async void TestUpdateUnauthorized()
        {
            //Arrange
            RouteController controller = CreateDefaultTestController(_routeService, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.Put(new Payloads.RouteUpdatePayload()
            {
                Id = 1,
                Title = "xd"
            });

            //Assert
            var contextResult = await _routeService.GetRouteById(1);
            Assert.IsType<UnauthorizedResult>(result);
            Assert.Equal("xd", contextResult.Title);
        }

        [Fact]
        public async void TestDeleteUnauthorized()
        {

            //Arrange
            RouteController controller = CreateDefaultTestController(_routeService, UserAuthorizationHandlerMode.FAIL);
            //Act
            var result = await controller.Delete(1);
            //Assert
            var contextResult = await _routeService.GetRouteById(1);
            Assert.IsType<UnauthorizedResult>(result);
            Assert.NotNull(contextResult);
        }
    }
}
