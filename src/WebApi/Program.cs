using CommandApp.Interfaces;
using Core.Settings;
using Framework.Extensions;
using Framework.Extensions.Swagger;
using Framework.Middlewares;
using MediatR;
using QueryApp.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddJwtAuthentication();
builder.Services.AddSwagger();
builder.Services.AddServices();
builder.Services.InitSettings(builder.Configuration);
builder.Services.AddKafkaProducer();
builder.Services.AddAerospikeClient();
builder.Services.AddCustomCorsSettings();
builder.Services.AddMediatR(typeof(ICommandApp).Assembly);
builder.Services.AddMediatR(typeof(IQueryApp).Assembly);
builder.Services.AddIdentityService();
builder.Services.AddRedis(builder.Configuration.GetSection(nameof(Settings)).Get<Settings>().RedisSettings);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("CORS");
}

app.UseAuthentication();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<FileMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();