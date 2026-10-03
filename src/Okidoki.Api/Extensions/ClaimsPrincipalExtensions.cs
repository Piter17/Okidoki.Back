using System.Security.Claims;

namespace Okidoki.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    const string _idClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
    public static long GetId(this ClaimsPrincipal user)
    {
        try
        {
            var id = user.Claims.Single(x => x.Type == _idClaim).Value;
            return long.Parse(id);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get userId from claims", ex);
        }

    }

    extension(ClaimsPrincipal claims)
    {
        public long UserId() => long.Parse(claims.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }
}
