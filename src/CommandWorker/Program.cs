using CommandApp.Interfaces;
using CommandWorker.Hubs;
using CommandWorker.Interfaces;
using CommandWorker.Services;
using Core.Services;
using Core.Settings;
using Framework.Extensions;
using Framework.Middlewares;
using MediatR;
using Microsoft.AspNetCore.Http.Connections;
using QueryApp.Interfaces;
using Repository.AeroSpike;

var builder = WebApplication.CreateBuilder(args);
builder.Services.InitSettings(builder.Configuration);
builder.Services.AddJwtAuthentication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddWorkerCustomCorsSettings();
builder.Services.AddKafkaProducer();
builder.Services.AddAerospikeClient();
builder.Services.AddRedis(builder.Configuration.GetSection(nameof(Settings)).Get<Settings>().RedisSettings);
builder.Services.AddSignalR(hubOptions => { hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(10); });
builder.Services.AddMediatR(typeof(ICommandWorker).Assembly);
builder.Services.AddMediatR(typeof(ICommandApp).Assembly);
builder.Services.AddMediatR(typeof(IQueryApp).Assembly);
builder.Services.AddSingleton<AerospikeRepository>();
builder.Services.AddSingleton<IdentityService>();
builder.Services.AddSingleton<IPushNotificationService, PushPushNotificationService>();
builder.Services.AddServices();

builder.Services.Scan(scan => scan
    .FromAssemblyOf<ICommandWorker>()
    .AddClasses(classes => classes.AssignableTo<IHostedService>())
    .AsImplementedInterfaces()
    .WithSingletonLifetime()
);

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
}

var app = builder.Build();

app.UseCors("CORS");
app.UseWebSockets();
app.UseAuthentication();
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthorization();

app.MapHub<NotificationHub>("/signalR",
    options =>
    {
        options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling |
                             HttpTransportType.ServerSentEvents;
    }
);

app.Run();