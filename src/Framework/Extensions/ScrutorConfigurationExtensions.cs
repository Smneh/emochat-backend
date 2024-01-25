using CommandApp.Interfaces;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using QueryApp.Interfaces;
using Repository.Interfaces;
using Services.Interfaces;
using Services.Services;

namespace Framework.Extensions;

public static class ScrutorConfigurationExtensions
{
    public static void AddServices(this IServiceCollection service)
    {
        service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        service.AddScoped<IFileService, FileService>();
        service.AddScoped<SecurityService>();

        service.Scan(
            scan => scan
                .FromAssembliesOf(typeof(ICore), typeof(IFramework), typeof(IQueryApp))
                .AddClasses(classes => classes.AssignableTo<IScopedDependency>())
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
        );

        service.Scan(
            scan => scan
                .FromAssembliesOf(typeof(ICore), typeof(IFramework), typeof(IQueryApp))
                .AddClasses(classes => classes.AssignableTo<IScopedDependencyAsSelf>())
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
        );

        service.Scan(
            scan => scan
                .FromAssembliesOf(typeof(ICore), typeof(IFramework), typeof(IQueryApp), typeof(ICommandApp))
                .AddClasses(classes => classes.AssignableTo<ISingletonDependencySelf>())
                .AsSelf()
                .WithSingletonLifetime()
        );

        service.Scan(
            scan => scan
                .FromAssembliesOf(typeof(ICore), typeof(IFramework), typeof(IQueryApp))
                .AddClasses(classes => classes.AssignableTo<ITransientDependency>())
                .AsSelfWithInterfaces()
                .WithTransientLifetime()
        );

        service.Scan(
            scan => scan
                .FromAssembliesOf(typeof(ICore), typeof(IFramework), typeof(IQueryApp))
                .AddClasses(classes => classes.AssignableTo<ISingletonDependency>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime()
        );

        service.Scan(
            scan => scan
                .FromAssemblyOf<IRepository>()
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                .AsSelf()
                .WithSingletonLifetime()
        );
    }
}