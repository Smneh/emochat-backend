using Core.Jwt;
using Microsoft.AspNetCore.Http;

namespace Core.Utilities.Assistant;

public class AccessTokenHelper
{
    public static string? GetClaim(HttpContext? httpContext, string type)
    {
        return httpContext?.User.Claims.SingleOrDefault(x => x.Type == type)?.Value;
    }

    public static CurrentUser GetCurrentUser(HttpContext? httpContext)
    {
        return new CurrentUser
        {
            Username = (GetClaim(httpContext, ClaimConstants.Username)),
            SessionId = GetClaim(httpContext, ClaimConstants.SessionId),
        };
    }

    public static string GetToken(HttpContext httpContext)
    {
        // Retrieve the Authorization header from the HttpContext
        var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();

        // Check if the header contains a valid JWT token
        if (authorizationHeader != null && string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
        {
            // No valid JWT token found in the header
            throw new UnauthorizedAccessException();
        }

        return authorizationHeader?["Bearer ".Length..]?.Trim() ?? throw new UnauthorizedAccessException();
    }
}