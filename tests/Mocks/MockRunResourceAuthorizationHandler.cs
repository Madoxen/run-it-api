using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Handlers;
using Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Tests.Mocks
{
    public class MockRunResourceAuthorizationHandlerSuccess :
        AuthorizationHandler<RunUserOwnershipRequirement, Run>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       RunUserOwnershipRequirement requirement,
                                                       Run run)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class MockRunResourceAuthorizationHandlerFail :
       AuthorizationHandler<RunUserOwnershipRequirement, Run>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       RunUserOwnershipRequirement requirement,
                                                       Run run)
        {
            context.Fail();
            return Task.CompletedTask;
        }
    }
}
