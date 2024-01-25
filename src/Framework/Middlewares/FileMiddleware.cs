using Microsoft.AspNetCore.Http;

namespace Framework.Middlewares;

public class FileMiddleware
{
    private readonly RequestDelegate _next;

    public FileMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Path.StartsWithSegments("/api/v1/File/Download"))
        {
            var token = httpContext.Request.Query["token"];
            httpContext.Request.Headers.Add("Authorization", $"Bearer {token}");
        }
        await _next(httpContext);
    }

}