using System.Security.Claims;

namespace PopcornBytes.Api.Security;

public static class IdentityUtils
{
    public static Guid GetUserIdClaim(ClaimsPrincipal claims) =>
        Guid.Parse(claims.Claims.Single(c => c.Type == "id").Value);
}