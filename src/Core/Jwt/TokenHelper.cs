using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Encryption;
using Core.Interfaces;
using Core.Utilities.Assistant;
using Entities.Models.Profile;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Core.Jwt;

public class TokenHelper : ITokenHelper, IScopedDependency
{
    private DateTime _accessTokenExpirationTime;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public JwtSecurityToken CreateToken(User user, string sessionId, DateTime expirationTime)
    {
        _accessTokenExpirationTime = expirationTime;

        return CreateJwt(Settings.Settings.AllSettings.JwtSettings.SecretKey, user, sessionId);
    }

    private JwtSecurityToken CreateJwt(string jwtSettingsSecretKey, User user, string sessionId)
    {
        var securityKey = SecurityKeyHelper.CreateSecurityKey(jwtSettingsSecretKey);
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        return _createJwtSecurityToken(user, signingCredentials, sessionId);
    }

    private JwtSecurityToken _createJwtSecurityToken(User user, SigningCredentials signingCredentials, string sessionId)
    {
        var jwt = new JwtSecurityToken
        (
            issuer: Settings.Settings.AllSettings.JwtSettings.Issuer,
            audience: Settings.Settings.AllSettings.JwtSettings.Audience,
            claims: _getClaimsList(user, sessionId),
            expires: _accessTokenExpirationTime,
            signingCredentials: signingCredentials
        );

        return jwt;
    }

    private static IEnumerable<Claim> _getClaimsList(User user, string sessionId)
    {
        List<Claim> claims = new()
        {
            new Claim(JwtRegisteredClaimNames.Sub, "jwt"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("UN", user.Username),
            new Claim("SI", sessionId),
            new Claim("RN", new Random().Next(0, 20).ToString()),
        };

        return claims;
    }

    private string GetClaim(string type)
    {
        var tokenString = AccessTokenHelper.GetToken(_httpContextAccessor.HttpContext);
        var token = new JwtSecurityToken(jwtEncodedString: tokenString);
        return token.Claims.First(c => c.Type == type).Value;
    }
    
    public static string GetUsername(HttpContext httpContext)
    {
        var tokenString = AccessTokenHelper.GetToken(httpContext);
        var token = new JwtSecurityToken(jwtEncodedString: tokenString);
        var encrypted = token.Claims.First(c => c.Type == "UN").Value;
        return encrypted;
    }

    public CurrentUser CurrentUser => AccessTokenHelper.GetCurrentUser(_httpContextAccessor.HttpContext);
}