using System.Collections.Generic;
using Api.Models;
using Xunit;
using System.Security.Claims;
using Api.Handlers;
using Microsoft.AspNetCore.Authorization;
using Api.Services;

namespace Api.Tests
{
    public class UserAuthorizationHandlerUnitTests
    {
        private enum ContextMode
        {
            SUCC,
            FAIL
        }

        public UserAuthorizationHandlerUnitTests()
        {
         
        }

        private AuthorizationHandlerContext ArrangeAuthContext(ContextMode mode = ContextMode.SUCC)
        {
            User resource = new User()
            {
                Id = 1
            };

            if (mode == ContextMode.FAIL)
                resource.Id = 2;

            var claims = new List<Claim>()
            {
                new Claim("sub", "1"),
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            var requirements = new[] { new SameUserIDRequirement() };

            return new AuthorizationHandlerContext(requirements, user, resource.Id);
        }

        [Fact]
        public async void TestValidUser()
        {
            //Arrange
            var authContext = ArrangeAuthContext();
            UserAuthorizationHandler handler = new UserAuthorizationHandler();

            //Act
            await handler.HandleAsync(authContext);

            //Assert
            Assert.True(authContext.HasSucceeded);
            Assert.False(authContext.HasFailed);
        }

        [Fact]
        public async void TestInvalidUser()
        {
            //Arrange
            var authContext = ArrangeAuthContext(ContextMode.FAIL);
            UserAuthorizationHandler handler = new UserAuthorizationHandler();

            
            //Act
            await handler.HandleAsync(authContext);

            //Assert
            Assert.True(authContext.HasFailed);
            Assert.False(authContext.HasSucceeded);
        }
    }
}
