using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EternalSolutions.Samples.B2C.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Internal;

namespace EternalSolutions.Samples.B2C.Api.NotesService
{
    public class ScopeAuthorizationRequirement : AuthorizationHandler<ScopeAuthorizationRequirement>, IAuthorizationRequirement
    {
        public readonly IEnumerable<string> Scopes;

        public ScopeAuthorizationRequirement(IEnumerable<string> scopes)
        {
            Scopes = scopes;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeAuthorizationRequirement requirement)
        {
            if (context.User != null && requirement.Scopes != null)
            {
                var scopeClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == Constants.ScopeClaim);
                if (scopeClaim != null)
                {
                    var matchedScopes = scopeClaim.Value.Split(' ')
                        .Join(requirement.Scopes, left => left, right => right, (left, right) => "Matched");
                    if (matchedScopes.Any())
                        context.Succeed(requirement);
                }
            }
            return TaskCache.CompletedTask;
        }
    }
}
