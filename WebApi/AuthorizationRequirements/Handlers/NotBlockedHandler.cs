using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApi.AuthorizationRequirements.Requirements;
using WebApi.Infrastructure.Components;

namespace WebApi.AuthorizationRequirements.Handlers;

public class NotBlockedHandler(DataComponent component) : AuthorizationHandler<NotBlockedRequirement>
{
    
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, NotBlockedRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            context.Fail();
            return;
        }

        var user = await component.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userIdClaim.Value);
        if (user == null || user.IsBlocked)
        {
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}
