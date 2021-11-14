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
using Api.Payloads;

namespace Api.Tests
{

    public class FriendRequestControllerUnitTests
    {
        // private enum UserAuthorizationHandlerMode
        // {
        //     SUCC = 0,
        //     FAIL = 1,
        // }

        // private async Task<FriendRequestController> Arrange(
        //     IUserRepository repo = null,
        //     IAuthorizationService authService = null,
        //     List<Claim> claims = null,
        //     UserAuthorizationHandlerMode handlerType = UserAuthorizationHandlerMode.SUCC)
        // {
        //     //Arrange user repository and add an user
        //     if (repo == null)
        //     {
        //         repo = new MockUserRepo();
        //         await repo.Add(new User() { Id = 1 });
        //     }


        //     //Arrange Authentication context
        //     IOptions<AuthenticationOptions> options = Options.Create(new AuthenticationOptions()
        //     {
        //         JWT = new JWTOptions()
        //         {
        //             Audience = "runit.co",
        //             Issuer = "runit.co",
        //             ExpiryMinutes = 20,
        //             Key = "aaabbbcccdddeeefffggghhhiiijjjkkk"
        //         }
        //     });

        //     if (claims == null)
        //     {
        //         claims = new List<Claim>()
        //         {
        //             new Claim("sub", "1"),
        //         };
        //     }

        //     var identity = new ClaimsIdentity(claims);
        //     var user = new ClaimsPrincipal(identity);

        //     //Create controller context and supply it with security details
        //     var context = new ControllerContext
        //     {
        //         HttpContext = new DefaultHttpContext
        //         {
        //             User = user,
        //         }
        //     };

        //     if (authService == null)
        //     {
        //         //Create authorization service
        //         authService = Builders.BuildAuthorizationService(services =>
        //         {
        //             if (handlerType == UserAuthorizationHandlerMode.SUCC)
        //                 services.AddScoped<IAuthorizationHandler, MockUserAuthorizationHandlerSuccess>();

        //             if (handlerType == UserAuthorizationHandlerMode.FAIL)
        //                 services.AddScoped<IAuthorizationHandler, MockUserAuthorizationHandlerFail>();

        //             services.AddAuthorization(options =>
        //             {
        //                 options.AddPolicy("CheckUserIDResourceAccess", policy => policy.Requirements.Add(new SameUserIDRequirement()));
        //             });
        //         });
        //     }


        //     //Arrange controller and supply it with context
        //     FriendRequestController controller = new FriendRequestController(repo, authService);
        //     controller.ControllerContext = context;

        //     return controller;
        // }


        // [Fact]
        // public async void TestGetEndpointWithExistingID()
        // {
        //     //Arrange
        //     FriendRequestController controller = await Arrange();

        //     //Act
        //     ActionResult<List<FriendPayload>> result = await controler.Get(1);

        //     //Assert
        //     Assert.IsType<List<FriendPayload>>(result.Value);
        // }


        // [Fact]
        // public async void TestGetEndpointWithNonExistingID()
        // {
        //     //Arrange
        //     FriendRequestController controller = await Arrange();

        //     //Act
        //     ActionResult<List<FriendPayload>> result = await controller.Get(2);

        //     //Assert
        //     Assert.IsType<NotFoundResult>(result.Result);
        //     Assert.Null(result.Value);
        // }


        // [Fact]
        // public async void TestDeleteEndpointWithExistingID()
        // {
        //     //Arrange
        //     var repo = new MockUserRepo();
        //     var user = new User() { Id = 1 };
        //     var friend = new User() { Id = 2 };
        //     user.Friends = new List<User>() { friend };
        //     friend.Friends = new List<User>() { user };
        //     await repo.Add(user);
        //     await repo.Add(friend);
        //     FriendRequestController controller = await Arrange(repo);

        //     //Act
        //     var result = await controller.Delete(1, 2);
        //     var repo_result = await repo.Get(1);

        //     //Assert
        //     Assert.Empty(repo_result.Friends);
        //     Assert.IsType<OkResult>(result);
        // }


        // [Fact]
        // public async void TestDeleteEndpointWithNonExistingID()
        // {
        //     //Arrange
        //     var repo = new MockUserRepo();
        //     var user = new User() { Id = 1 };
        //     var friend = new User() { Id = 2 };
        //     user.Friends = new List<User>() { friend };
        //     friend.Friends = new List<User>() { user };
        //     await repo.Add(user);
        //     await repo.Add(friend);
        //     FriendRequestController controller = await Arrange(repo);


        //     //Act
        //     var result = await controller.Delete(1, 3);

        //     //Assert
        //     Assert.IsType<NotFoundObjectResult>(result);
        // }


        // [Fact]
        // public async void TestGetUnauthorized()
        // {
        //     //Arrange
        //     FriendRequestController controller = await Arrange(handlerType: UserAuthorizationHandlerMode.FAIL);
        //     //Act
        //     var result = await controller.Get(1);
        //     //Assert
        //     Assert.IsType<UnauthorizedResult>(result.Result);
        //     Assert.Null(result.Value);
        // }


        // [Fact]
        // public async void TestDeleteUnauthorized()
        // {
        //     //Arrange
        //     var repo = new MockUserRepo();
        //     var user = new User() { Id = 1 };
        //     var friend = new User() { Id = 2 };
        //     user.Friends = new List<User>() { friend };
        //     friend.Friends = new List<User>() { user };
        //     await repo.Add(user);
        //     await repo.Add(friend);
        //     await repo.Add(new User() { Id = 1, Weight = 1 });
        //     FriendRequestController controller = await Arrange(repo: repo, handlerType: UserAuthorizationHandlerMode.FAIL);
        //     //Act
        //     var result = await controller.Delete(1, 2);
        //     //Assert
        //     Assert.IsType<UnauthorizedResult>(result);
        //     Assert.NotNull(await repo.Get(1));
        // }

    }
}

