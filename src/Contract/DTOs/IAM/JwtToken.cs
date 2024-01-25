using System.IdentityModel.Tokens.Jwt;

namespace Contract.DTOs.IAM;

public class UserToken
{
    private JwtSecurityToken token;

    public UserToken(JwtSecurityToken token)
    {
        this.token = token;
    }

    public UserToken()
    {
    }

    public DateTime? ValidTo => token?.ValidTo;

    public string? access_token => token == null ? null : new JwtSecurityTokenHandler().WriteToken(this.token);
}