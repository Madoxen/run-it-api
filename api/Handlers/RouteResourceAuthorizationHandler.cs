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
        private readonly RouteShareService _shareService;

        public RouteResourceAuthorizationHandler(RouteShareService shareService)
        {
            _shareService = shareService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       RouteUserOwnershipRequirement requirement,
                                                       Route route)
        {
            int userId = int.Parse(context.User?.Claims?.FirstOrDefault(x => x.Type == "sub")?.Value);

            if (route.UserId == userId)
            {
                context.Succeed(requirement);
                return;
            }

            var shareResult = await _shareService.GetRouteShare(route.Id, userId);
            if (shareResult.Value == null)
            {
                context.Fail();
                return;
            }

            var share = shareResult.Value;
            if (share.SharedToId == userId)
            {
                context.Succeed(requirement);
                return;
            }
            context.Fail();
            return;
        }
    }

    public class RouteUserOwnershipRequirement : IAuthorizationRequirement { }
}
