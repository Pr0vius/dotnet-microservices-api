using ms.user.api.Helpers;
using ms.user.api.Responses;
using System.Net;

namespace ms.user.api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (ExceptionResponse ex)
            {
                // En caso de error, manejar la excepción y devolver una respuesta estructurada
                var errorResponse = new ApiResponse<object>(null, ex.Message, ex.StatusCode);
                httpContext.Response.StatusCode = (int)ex.StatusCode;
                await httpContext.Response.WriteAsJsonAsync(errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
                var errorResponse = new ApiResponse<object>(null, ex.Message, HttpStatusCode.InternalServerError);
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpContext.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}
