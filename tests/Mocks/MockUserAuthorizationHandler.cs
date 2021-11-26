using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Handlers;
using Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Tests.Mocks
{
    public class MockUserAuthorizationHandlerSuccess :
        AuthorizationHandler<SameUserIDRequirement, int>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       SameUserIDRequirement requirement,
                                                       int userId)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class MockUserAuthorizationHandlerFail :
       AuthorizationHandler<SameUserIDRequirement, int>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       SameUserIDRequirement requirement,
                                                       int userId)
        {
            context.Fail();
            return Task.CompletedTask;
        }
    }
}
