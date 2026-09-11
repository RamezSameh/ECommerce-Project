using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ECommerce.API.Middleware;

/// <summary>
/// Centralized exception handling. Converts any thrown exception (including
/// our custom NotFoundException, ValidationException, etc.) into a consistent
/// <see cref="ApiResponse{T}"/> JSON payload with an appropriate HTTP status.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception during request {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(exception.Message)),
            BusinessException => (HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(exception.Message)),
            ECommerce.Application.Exceptions.ValidationException validation =>
                (HttpStatusCode.BadRequest,
                 ApiResponse<object>.Fail(validation.Errors)),
            UnauthorizedException => (HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail(exception.Message)),
            _ => (HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred. Please try again later."))
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}