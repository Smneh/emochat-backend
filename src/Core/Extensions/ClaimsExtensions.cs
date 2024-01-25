using System.IdentityModel.Tokens.Jwt;

namespace Core.Extensions;

public static class ClaimsExtensions
{
    public static T GetClaimById<T>(this string jwtToken, string inputClaim)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadToken(jwtToken) as JwtSecurityToken;


        var claimValue = token!.Claims.First(claim => claim.Type == inputClaim).Value.ToString();
        return (T)Convert.ChangeType(claimValue, typeof(T));
    }
}