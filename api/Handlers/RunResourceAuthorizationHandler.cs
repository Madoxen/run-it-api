using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace Api.Handlers
{
    public class RunResourceAuthorizationHandler :
        AuthorizationHandler<RunUserOwnershipRequirement, Run>
    {

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       RunUserOwnershipRequirement requirement,
                                                       Run run)
        {
            int userId = int.Parse(context.User?.Claims?.FirstOrDefault(x => x.Type == "sub")?.Value);
            if (run.UserId == userId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            context.Fail();
            return Task.CompletedTask;
        }
    }

    public class RunUserOwnershipRequirement : IAuthorizationRequirement { }
}
