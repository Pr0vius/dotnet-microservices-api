using NLog;
using System.Diagnostics;
using System.Text;

namespace ms.task.api.Middlewares
{
    public class RequestLoggingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.Headers.TryGetValue("X-Trace-ID", out var traceId);
            traceId = !string.IsNullOrEmpty(traceId) ? traceId : Guid.NewGuid().ToString();

            context.Items["TraceId"] = traceId;
            context.Response.Headers["X-Trace-ID"] = traceId;


            using (ScopeContext.PushProperty("traceId", traceId))
            {
                var request = context.Request;

                request.EnableBuffering();
                var bodyAsText = await new StreamReader(request.Body).ReadToEndAsync();
                request.Body.Position = 0;

                var stopwatch = Stopwatch.StartNew();

                var logMessage = new StringBuilder();
                logMessage.AppendLine($"TraceID: {traceId}");
                logMessage.AppendLine($"Method: {request.Method}");
                logMessage.AppendLine($"URL: {request.Path}");
                logMessage.AppendLine($"Query: {request.QueryString}");

                foreach (var header in request.Headers)
                {
                    logMessage.AppendLine($"Header: {header.Key} = {header.Value}");
                }

                if (!string.IsNullOrEmpty(bodyAsText))
                {
                    logMessage.AppendLine($"Body: {bodyAsText}");
                }

                _logger.Info(logMessage.ToString());

                await _next(context);
                stopwatch.Stop();

                var responseLog = new StringBuilder();
                responseLog.AppendLine($"TraceID: {traceId}");
                responseLog.AppendLine($"StatusCode: {context.Response.StatusCode}");
                responseLog.AppendLine($"ProcessingTime: {stopwatch.ElapsedMilliseconds} ms");

                _logger.Info(responseLog.ToString());
            }
        }
    }
}
