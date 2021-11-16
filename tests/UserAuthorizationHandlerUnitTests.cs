using System.Collections.Generic;
using Api.Models;
using Xunit;
using System.Security.Claims;
using Api.Handlers;
using Microsoft.AspNetCore.Authorization;

namespace Api.Tests
{
    public class UserAuthorizationHandlerUnitTests
    {
        private enum ContextMode
        {
            SUCC,
            FAIL
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

            return new AuthorizationHandlerContext(requirements, user, resource);
        }

        [Fact]
        public void TestValidUser()
        {
            //Arrange
            var authContext = ArrangeAuthContext();
            UserAuthorizationHandler handler = new UserAuthorizationHandler();

            //Act
            handler.HandleAsync(authContext);

            //Assert
            Assert.True(authContext.HasSucceeded);
            Assert.False(authContext.HasFailed);
        }

        [Fact]
        public void TestInvalidUser()
        {
            //Arrange
            var authContext = ArrangeAuthContext(ContextMode.FAIL);
            UserAuthorizationHandler handler = new UserAuthorizationHandler();

            //Act
            handler.HandleAsync(authContext);

            //Assert
            Assert.True(authContext.HasFailed);
            Assert.False(authContext.HasSucceeded);
        }
    }
}
