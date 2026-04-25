using HealthCare.Shared.Exceptions;
using HealthCare.Shared.Wrappers;

namespace HeathCare.Api.Middlewares;

public class ExceptionMiddleware(RequestDelegate next)
{

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
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
            await WriteResponse(context, 500,
                ApiResponse<object>.Error(ExceptionMessage.INTERNAL_ERROR,
                    new List<string> { ex.Message }));
        }
    }
    
    private static async Task WriteResponse<T>(HttpContext context, int statusCode, ApiResponse<T> response)
    {
        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
    
}