using FluentValidation;
using HealthCare.Shared.Exceptions;
using HealthCare.Shared.Wrappers;

namespace HeathCare.Api.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
            await WriteResponse(context, 400,
                ApiResponse<object>.Error("Los datos enviados no son válidos.", errors));
        }
        catch (AppExceptions.NotFoundException ex)
        {
            await WriteResponse(context, 404,
                ApiResponse<object>.Error(ex.Message));
        }
        catch (AppExceptions.BadRequestException ex)
        {
            await WriteResponse(context, 400,
                ApiResponse<object>.Error(ex.Message, ex.Errors));
        }
        catch (AppExceptions.UnauthorizedException ex)
        {
            await WriteResponse(context, 401,
                ApiResponse<object>.Error(ex.Message));
        }
        catch (AppExceptions.ForbiddenException ex)
        {
            await WriteResponse(context, 403,
                ApiResponse<object>.Error(ex.Message));
        }
        catch (AppExceptions.ConflictException ex)
        {
            await WriteResponse(context, 409,
                ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await WriteResponse(context, 500,
                ApiResponse<object>.Error(ExceptionMessage.INTERNAL_ERROR));
        }
    }

    private static async Task WriteResponse<T>(HttpContext context, int statusCode, ApiResponse<T> response)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}