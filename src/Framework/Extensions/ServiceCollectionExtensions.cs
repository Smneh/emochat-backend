using Microsoft.Extensions.DependencyInjection;

namespace Framework.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddCustomCorsSettings(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: "CORS",
                corsPolicyBuilder =>
                {
                    corsPolicyBuilder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });
    }
    
    public static void AddWorkerCustomCorsSettings(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CORS", 
                builder => 
                    builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetIsOriginAllowed(_ => true));
        });
    }
}