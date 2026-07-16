using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LibrarySys.Authorization
{
    public class SameMemberHandler : AuthorizationHandler<SameMemberRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SameMemberHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,SameMemberRequirement requirement)
        {
            if (context.User.IsInRole("Admin") || context.User.IsInRole("Librarian"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            string? memberIdClaim = context.User.FindFirstValue("memberId");

            if (!int.TryParse(memberIdClaim, out int authenticatedMemberID))
            {
                return Task.CompletedTask;
            }

            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return Task.CompletedTask;
            }

            if (!httpContext.Request.RouteValues.TryGetValue("memberID", out var routeMemberIdValue))
            {
                return Task.CompletedTask;
            }

            if (!int.TryParse(routeMemberIdValue?.ToString(), out int requestedMemberID))
            {
                return Task.CompletedTask;
            }

            if (authenticatedMemberID == requestedMemberID)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}