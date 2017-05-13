using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;

namespace EternalSolutions.Samples.B2C.Api.NotesService
{
    public static class AuthorizationPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder @this, string scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            @this.Requirements.Add(new ScopeAuthorizationRequirement(new[] { scope }));
            return @this;
        }

        public static AuthorizationPolicyBuilder RequireScopesAll(this AuthorizationPolicyBuilder @this, IEnumerable<string> scopes)
        {
            foreach (var scope in scopes)
                @this.RequireScope(scope);
            return @this;
        }

        public static AuthorizationPolicyBuilder RequireScopesAny(this AuthorizationPolicyBuilder @this, IEnumerable<string> scopes)
        {
            @this.Requirements.Add(new ScopeAuthorizationRequirement(scopes));
            return @this;
        }
    }
}
