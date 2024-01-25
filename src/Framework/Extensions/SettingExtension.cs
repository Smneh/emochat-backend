using Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Extensions;

public static class SettingExtension
{
    public static void InitSettings(this IServiceCollection serviceDescriptors, IConfiguration configuration)
    {
        var settings = configuration.GetSection(nameof(Settings)).Get<Settings>();
        Settings.Set(settings);

        serviceDescriptors.Configure<Settings>(configuration.GetSection(nameof(Settings)));
    }
}