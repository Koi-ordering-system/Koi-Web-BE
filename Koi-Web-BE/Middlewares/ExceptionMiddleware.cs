using System.Text.Json;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Middlewares;

public class ExceptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private readonly IDictionary<Type, Action<HttpContext, Exception>> _exceptionHandlers = new Dictionary<Type, Action<HttpContext, Exception>>
    {
        // Note: Handle every exception you throw here
        
        // a NotFoundException is thrown when the resource requested by the client
        // cannot be found on the resource server
        { typeof(NotFoundException), HandleNotFoundException },
        
        // a ConflictException is usually thrown when there is a PUT request, say it
        // tries to update a resource and the optimistic lock versioning is conflict
        // aka a race condition
        { typeof(ConflictException), HandleConflictException },
        
        // a RequestValidationException is thrown when a request does not comply with
        // its validator predefined by the developer in its corresponding queries/commands
        // This is for the ValidationBehavior to throw only, do not manually throw this
        { typeof(RequestValidationException), HandleRequestValidationException },
        
        // an UnauthorizedException is thrown when the requests going through mediatR
        // pipeline does not satisfy the requirements predefined by the developers
        // This can be thrown in authorization validation before executing the authorization logic
        { typeof(UnauthorizedException), HandleUnauthorizedAccessException },
        
        // a BadRequestException is thrown when the executing of a request finds that
        // aside from the primary resource being requested, additional request properties
        // cannot be used to complete the execution, say when a parent entity does not exist
        // when trying to update/modify the primary entity
        { typeof(BadRequestException), HandleBadRequestException },
        
        // a BadRequestException is thrown when the executing of a request finds that
        // aside from the primary resource being requested, additional request properties
        // cannot be used to complete the execution, say when a parent entity does not exist
        // when trying to update/modify the primary entity
        // { typeof(BadHttpRequestException), HandleBadHttpRequestException },
    };

    private Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var type = ex.GetType();
        if (_exceptionHandlers.TryGetValue(type, out var handler))
        {
            handler.Invoke(context, ex);
            return Task.CompletedTask;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        Console.WriteLine(ex.ToString());
        return Task.CompletedTask;
    }


    private static async void HandleNotFoundException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await WriteExceptionMessageAsync(context, ex);
    }

    private static async void HandleConflictException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await WriteExceptionMessageAsync(context, ex);
    }

    private static async void HandleRequestValidationException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var rve = ex as RequestValidationException;
        var data = rve!.Errors!.ToDictionary(vf => vf.PropertyName.ToLower(), vf => vf.ErrorMessage);

        var result = Result<Dictionary<string, string>>.Fail(ex) with
        {
            Data = data
        };
        await context.Response.Body.WriteAsync(SerializeToUtf8BytesWeb(result));
    }

    private static async void HandleUnauthorizedAccessException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await WriteExceptionMessageAsync(context, ex);
    }

    private static async void HandleBadRequestException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await WriteExceptionMessageAsync(context, ex);
    }

    private static async void HandleBadHttpRequestException(HttpContext context, Exception _)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var e = new Exception("invalid request format");
        await context.Response.Body.WriteAsync(SerializeToUtf8BytesWeb(Result<string>.Fail(e)));
    }

    private static async Task WriteExceptionMessageAsync(HttpContext context, Exception ex)
    {
        await context.Response.Body.WriteAsync(SerializeToUtf8BytesWeb(Result<string>.Fail(ex)));
    }

    private static byte[] SerializeToUtf8BytesWeb<T>(T value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}