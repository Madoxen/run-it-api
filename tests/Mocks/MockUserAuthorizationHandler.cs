using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Handlers;
using Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Test.Mocks
{
    public class MockUserAuthorizationHandlerSuccess :
        AuthorizationHandler<SameUserIDRequirement, User>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       SameUserIDRequirement requirement,
                                                       User user)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class MockUserAuthorizationHandlerFail :
       AuthorizationHandler<SameUserIDRequirement, User>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       SameUserIDRequirement requirement,
                                                       User user)
        {
            context.Fail();
            return Task.CompletedTask;
        }
    }
}
