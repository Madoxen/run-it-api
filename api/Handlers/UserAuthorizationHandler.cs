using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Handlers
{
    public class UserAuthorizationHandler :
        AuthorizationHandler<SameUserIDRequirement, User>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       SameUserIDRequirement requirement,
                                                       User user)
        {
            
            if (context.User?.Claims?.FirstOrDefault(x => x.Type == "sub")?.Value == user.Id.ToString())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public class SameUserIDRequirement : IAuthorizationRequirement { }
}
