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
    public class RouteShareControllerUnitTests
    {
        public RouteShareControllerUnitTests()
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

        private RouteShareController CreateDefaultTestController(IRouteService routeService,
                                                                 IRouteShareService routeShareService,
                                                                 UserAuthorizationHandlerMode authMode = UserAuthorizationHandlerMode.SUCC)
        {
            RouteShareController controller = new RouteShareController(routeShareService, routeService, ArrangeAuthService(handlerType: authMode));
            controller.ControllerContext = ArrangeControllerContext();
            return controller;
        }

        [Fact]
        public async void TestGetUserShares()
        {
            //Arrange
            var routeService = new Mock<IRouteService>();
            var routeShareService = new Mock<IRouteShareService>();
            routeShareService.Setup(x => x.GetSharesForUser(1)).ReturnsAsync(new List<RouteShare>());
            RouteShareController controller = CreateDefaultTestController(
                routeService.Object,
                routeShareService.Object);

            //Act
            ActionResult<List<RouteShareGetPayload>> result = await controller.Get(1);

            //Assert
            Assert.IsType<List<RouteShareGetPayload>>(result.Value);
        }


        [Fact]
        public async void TestPostShare()
        {
            //Arrange
            var routeService = new Mock<IRouteService>();
            var routeShareService = new Mock<IRouteShareService>();
            routeShareService.Setup(x => x.GetSharesForUser(1)).ReturnsAsync(new List<RouteShare>());
            RouteShareController controller = CreateDefaultTestController(
                routeService.Object,
                routeShareService.Object);

            //Act
            ActionResult<List<RouteShareGetPayload>> result = await controller.Get(1);

            //Assert
            Assert.IsType<List<RouteShareGetPayload>>(result.Value);
        }


        [Fact]
        public async void TestGetUnauthorized()
        {
            //Arrange
            var routeService = new Mock<IRouteService>();
            var routeShareService = new Mock<IRouteShareService>();
            routeService.Setup(x => x.GetRouteById(1)).ReturnsAsync(new Route());
            RouteShareController controller = CreateDefaultTestController(routeService.Object, routeShareService.Object, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async void TestPostUnauthorized()
        {
            //Arrange
            var routeService = new Mock<IRouteService>();
            var routeShareService = new Mock<IRouteShareService>();
            routeService.Setup(x => x.GetRouteById(1)).ReturnsAsync(new Route());
            RouteShareController controller = CreateDefaultTestController(routeService.Object, routeShareService.Object, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.Post(1,1);

            //Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async void TestDeleteUnauthorized()
        {

            //Arrange
            var routeService = new Mock<IRouteService>();
            var routeShareService = new Mock<IRouteShareService>();
            routeService.Setup(x => x.GetRouteById(1)).ReturnsAsync(new Route());
            RouteShareController controller = CreateDefaultTestController(routeService.Object, routeShareService.Object, UserAuthorizationHandlerMode.FAIL);

            //Act
            var result = await controller.Delete(1,1);

            //Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
