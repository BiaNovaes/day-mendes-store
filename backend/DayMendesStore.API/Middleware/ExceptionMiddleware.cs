using System.Net;
using System.Text.Json;
using DayMendesStore.Application.Exceptions;

namespace DayMendesStore.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado capturado pelo middleware global: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            BusinessException => HttpStatusCode.BadRequest,
            NotFoundException => HttpStatusCode.NotFound,
            UnauthorizedException => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError
        };

        var message = exception switch
        {
            BusinessException => exception.Message,
            NotFoundException => exception.Message,
            UnauthorizedException => exception.Message,
            _ => _env.IsDevelopment()
                ? $"{exception.GetType().Name}: {exception.Message}"
                : "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde."
        };

        context.Response.StatusCode = (int)statusCode;

        object response = _env.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError
            ? new
            {
                status = (int)statusCode,
                message,
                detail = exception.InnerException?.Message ?? exception.Message,
                timestamp = DateTime.UtcNow
            }
            : new
            {
                status = (int)statusCode,
                message,
                timestamp = DateTime.UtcNow
            };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
