using System.Text;
using Core.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Framework.Extensions;

public static class JwtExtensions
{
    public static void AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var secretKey = Encoding.UTF8.GetBytes(Settings.AllSettings.JwtSettings.SecretKey);
            var encryptionKey = Encoding.UTF8.GetBytes(Settings.AllSettings.JwtSettings.EncryptionKey);

            TokenValidationParameters validationParameters = new()
            {
                ClockSkew = TimeSpan.Zero, //impact to expire time & not before - default is 5 min and 5 min after expire also valid
                RequireSignedTokens = true, // token have signature

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),

                RequireExpirationTime = true,
                ValidateLifetime = true,

                ValidateAudience = true, // default: false
                ValidAudience = Settings.AllSettings.JwtSettings.Audience,

                ValidateIssuer = true, // default: false
                ValidIssuer = Settings.AllSettings.JwtSettings.Issuer,

                TokenDecryptionKey = new SymmetricSecurityKey(encryptionKey)
            };

            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = validationParameters;
        });
    }
}