using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace Api.Handlers
{
    public class RouteShareResourceAuthorizationHandler :
        AuthorizationHandler<RouteShareUserOwnershipRequirement, RouteShare>
    {

        public RouteShareResourceAuthorizationHandler()
        {

        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       RouteShareUserOwnershipRequirement requirement,
                                                       RouteShare share)
        {
            int userId = int.Parse(context.User?.Claims?.FirstOrDefault(x => x.Type == "sub")?.Value);

            //If you are selected for a share - PASSED
            if (share.SharedToId == userId)
            {
                context.Succeed(requirement);
                return;
            }

            //If you are an owner of a share - PASSED
            if (share.Route.UserId == userId)
            {
                context.Succeed(requirement);
                return;
            }

            context.Fail();
            return;
        }
    }

    public class RouteShareUserOwnershipRequirement : IAuthorizationRequirement { }
}
