using System.IdentityModel.Tokens.Jwt;
using Entities.Models.Profile;

namespace Core.Jwt;

public interface ITokenHelper
{
    CurrentUser CurrentUser { get; }
    JwtSecurityToken CreateToken(User user, string sessionId, DateTime expirationTime);
}