using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace Api.Handlers
{
    public class RouteResourceAuthorizationHandler :
        AuthorizationHandler<RouteUserOwnershipRequirement, Route>
    {

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       RouteUserOwnershipRequirement requirement,
                                                       Route route)
        {
            int userId = int.Parse(context.User?.Claims?.FirstOrDefault(x => x.Type == "sub")?.Value);
            if (route.UserId == userId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            context.Fail();
            return Task.CompletedTask;
        }
    }

    public class RouteUserOwnershipRequirement : IAuthorizationRequirement { }
}
