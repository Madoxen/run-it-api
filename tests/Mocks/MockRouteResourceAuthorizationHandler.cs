using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Handlers;
using Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Tests.Mocks
{
    public class MockRouteResourceAuthorizationHandlerSuccess :
        AuthorizationHandler<RouteUserOwnershipRequirement, Route>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       RouteUserOwnershipRequirement requirement,
                                                       Route route)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class MockRouteResourceAuthorizationHandlerFail :
       AuthorizationHandler<RouteUserOwnershipRequirement, Route>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       RouteUserOwnershipRequirement requirement,
                                                       Route route)
        {
            context.Fail();
            return Task.CompletedTask;
        }
    }
}
