// using System.Collections.Generic;
// using Api.Models;
// using Xunit;
// using Api.Controllers;
// using Microsoft.AspNetCore.Mvc;
// using Api.Configuration.Options;
// using Api.Payloads;
// using Microsoft.Extensions.Options;
// using System.Security.Claims;
// using Microsoft.AspNetCore.Http;
// using Api.Tests.Utils;
// using Microsoft.EntityFrameworkCore;

// namespace Api.Testss
// {
//     public class AuthControllerUnitTests
//     {
//         public AuthControllerUnitTests()
//         {
//             Seed();
//         }

//         private enum UserAuthorizationHandlerMode
//         {
//             SUCC = 0,
//             FAIL = 1,
//         }

//         protected DbContextOptions<ApiContext> ContextOptions { get; } = Builders.BuildDefaultDbContext();

//         private void Seed()
//         {
//             using (var context = new ApiContext(ContextOptions))
//             {
//                 context.Database.EnsureDeleted();
//                 context.Database.EnsureCreated();

//                 var user = new User()
//                 {
//                     Id = 1,
//                 };

//                 context.Users.Add(user);
//                 context.SaveChanges();
//             }
//         }

//         [Fact]
//         public async void TestGetRefreshTokenEndpointValidToken()
//         {
//             //Arrange
//             using (ApiContext context = new ApiContext(ContextOptions))
//             {
//                 IOptions<AuthenticationOptions> options = Options.Create(new AuthenticationOptions()
//                 {
//                     JWT = new JWTOptions()
//                     {
//                         Audience = "runit.co",
//                         Issuer = "runit.co",
//                         ExpiryMinutes = 20,
//                         Key = "aaabbbcccdddeeefffggghhhiiijjjkkk"
//                     }
//                 });

//                 var claims = new List<Claim>()
//             {
//                 new Claim("sub", "1"),
//             };
//                 var identity = new ClaimsIdentity(claims);
//                 var user = new ClaimsPrincipal(identity);
//                 var controllerContext = new ControllerContext
//                 {
//                     HttpContext = new DefaultHttpContext
//                     {
//                         User = user,
//                     }
//                 };

//                 AuthController controller = new AuthController(context, null, options);
//                 controller.ControllerContext = controllerContext;
                
//                 //Act
//                 ActionResult<JWTAuthPayload> result = await controller.GetRefereshToken();

//                 //Assert
//                 Assert.NotNull(result);
//                 Assert.NotNull(result.Value);
//             }
//         }
//     }
// }
