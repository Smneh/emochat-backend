using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Core.Encryption;

public class SecurityKeyHelper
{
    public static SecurityKey CreateSecurityKey(string secretKey)
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
    }
}