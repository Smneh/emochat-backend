using System.Net;
using System.Security.Authentication;
using Core.Enums;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Framework.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (AppException exception)
        {
            _logger.LogError(
                exception,
                "An application exception occurred." + "{MessageFa} {Path} {Username} {Workspace} {CustomObject}",
                exception.MessageEn, httpContext.Request.Path.Value,
                httpContext.Request.Headers["Username"].ToString(),
                httpContext.Request.Headers["Workspace"].ToString(),
                JsonConvert.SerializeObject(exception.CustomObject)
            );

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var result = new ErrorResponse
            {
                MessageEn = exception.Message.ToString(), 
                ErrorCode = (int)exception.Message
            };

            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }
        catch (AuthenticationException exception)
        {
            _logger.LogError(
                exception,
                "An AuthenticationException exception occurred." + "{MessageFa} {Path} {Username} {Workspace} {CustomObject}",
                Messages.AuthenticationError.ToDescription(), httpContext.Request.Path.Value,
                httpContext.Request.Headers["Username"].ToString(),
                httpContext.Request.Headers["Workspace"].ToString(),
                JsonConvert.SerializeObject(exception)
            );

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = 401;

            var result = new ErrorResponse
            {
                MessageEn = Messages.AuthenticationError.ToString(),
                ErrorCode = 401
            };

            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An application exception occurred." + "{MessageFa} {Path} {Username} {Workspace} {CustomObject}",
                Messages.ServerError.ToDescription(), httpContext.Request.Path.Value,
                httpContext.Request.Headers["Username"].ToString(),
                httpContext.Request.Headers["Workspace"].ToString(),
                JsonConvert.SerializeObject(exception)
            );


            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var result = new ErrorResponse
            {
                MessageEn = Messages.ServerError.ToString(),
                ErrorCode = (int)Messages.ServerError
            };

            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }
    }

    class ErrorResponse
    {
        public string MessageEn { get; set; } = default!;
        public int ErrorCode { get; set; }
    }
}