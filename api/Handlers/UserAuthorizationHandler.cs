using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace Api.Handlers
{
    public class UserAuthorizationHandler :
        AuthorizationHandler<SameUserIDRequirement, int>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       SameUserIDRequirement requirement,
                                                       int userId)
        {
            if (context.User?.Claims?.FirstOrDefault(x => x.Type == "sub")?.Value == userId.ToString())
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            context.Fail();
            return Task.CompletedTask;
        }
    }

    public class SameUserIDRequirement : IAuthorizationRequirement { }
}
