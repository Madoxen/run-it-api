using System;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Authorization;

public class UserAuthorizationHandler :
    AuthorizationHandler<SameUserIDRequirement, User>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   SameUserIDRequirement requirement,
                                                   User user)
    {
        Console.WriteLine($"xd: {context.User.Identity?.Name}");
        if (context.User.Identity?.Name == user.Id.ToString())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class SameUserIDRequirement : IAuthorizationRequirement { }
