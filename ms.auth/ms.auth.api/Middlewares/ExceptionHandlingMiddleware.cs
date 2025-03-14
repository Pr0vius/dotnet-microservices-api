using ms.auth.api.Helpers;
using ms.auth.api.Responses;
using System.Net;

namespace ms.auth.api.Middlewares
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;



        public async Task InvokeAsync(HttpContext httpContext)
        {
            var traceId = httpContext.Items.ContainsKey("TraceId") ? httpContext.Items["TraceId"]!.ToString() : Guid.NewGuid().ToString();
            httpContext.Response.Headers["X-Trace-ID"] = traceId;


            try
            {
                await _next(httpContext);
            }
            catch (ExceptionResponse ex)
            {
                _logger.LogError(
                    "Handled Exception: {Message}. | " +
                    "StatusCode: {StatusCode}. | " +
                    "Path: {Path}. | " +
                    "Method: {Method}. | " +
                    "QueryString: {QueryString}. | " +
                    "User: {User}. | " +
                    "ClientIP: {ClientIP}. | " +
                    "TraceID: {TraceID}.",
                    ex.Message,
                    ex.StatusCode,
                    httpContext.Request.Path,
                    httpContext.Request.Method,
                    httpContext.Request.QueryString,
                    httpContext.User?.Identity?.Name ?? "Anonymous",
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    traceId
                );

                var errorResponse = new ApiResponse<object>(new { }, ex.Message, ex.StatusCode);
                httpContext.Response.StatusCode = (int)ex.StatusCode;
                await httpContext.Response.WriteAsJsonAsync(errorResponse);
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? "None";
                _logger.LogCritical(ex,
                    "Unhandled Exception: {Message}. | " +
                    "Path: {Path}. | " +
                    "Method: {Method}. | " +
                    "QueryString: {QueryString}. | " +
                    "User: {User}. | " +
                    "ClientIP: {ClientIP}. | " +
                    "TraceID: {TraceID}. | " +
                    "StackTrace: {StackTrace}. | " +
                    "InnerException: {InnerException}.",
                    ex.Message,
                    httpContext.Request.Path,
                    httpContext.Request.Method,
                    httpContext.Request.QueryString,
                    httpContext.User?.Identity?.Name ?? "Anonymous",
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    traceId,
                    ex.StackTrace,
                    innerExceptionMessage
                );

                var errorResponse = new ApiResponse<object>(new { }, ex.Message, HttpStatusCode.InternalServerError);
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpContext.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}
