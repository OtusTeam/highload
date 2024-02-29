using Azure;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Text.Json;
using OtusSocialNetwork.DataClasses.Responses;

namespace OtusSocialNetwork.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            _logger.LogError(error, "Error");
            switch (error)
            {
                case Exceptions.UnauthorizedException e:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                case Exceptions.ForbiddenException e:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                case Exceptions.ApiException e:
                    // custom application error
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case Exceptions.ValidationCustomException e:
                    // custom application error
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case KeyNotFoundException e:
                    // not found error
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                default:
                    // unhandled error
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }
            var result = JsonSerializer.Serialize(new ErrorRes(error.Message), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await response.WriteAsync(result);
        }
    }
}
