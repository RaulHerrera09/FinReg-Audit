using System.Text.Json;
using FinReg.Domain.Exceptions;
using FluentValidation;

namespace FinReg.API.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await Write(context, 422, "Validation failed.",
                ex.Errors.Select(e => e.ErrorMessage).ToList());
        }
        catch (AccountNotFoundException ex)
        {
            await Write(context, 404, ex.Message);
        }
        catch (InvalidTransactionException ex)
        {
            await Write(context, 404, ex.Message);
        }
        catch (DomainException ex)
        {
            await Write(context, 400, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await Write(context, 500, "An unexpected error occurred.");
        }
    }

    private static async Task Write(HttpContext ctx, int status, string error,
        IReadOnlyList<string>? validationErrors = null)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(
            new { success = false, error, validationErrors },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await ctx.Response.WriteAsync(body);
    }
}
