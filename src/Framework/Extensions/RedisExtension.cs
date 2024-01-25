using Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Framework.Extensions
{
    public static class RedisExtension
    {
        public static void AddRedis(this IServiceCollection services, RedisSettings redisSettings)
        {
            var multiplexer = ConnectionMultiplexer.Connect(redisSettings.Host + ":" + redisSettings.Port);

            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        }
    }
}
