using Core.Jwt;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Extensions;

public static class IdentityExtension
{
    public static void AddIdentityService(this IServiceCollection servives)
    {
        servives.AddScoped<IdentityService>(serviceProvider =>
        {
            var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var username = TokenHelper.GetUsername(httpContext!);

            return new IdentityService(username);
        });
    }
}