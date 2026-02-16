using System.Net;
using System.Text.Json;

namespace CMaaS.Backend.Middlewares
{
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

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Forward the request to the next middleware (Controller)
                await _next(context);
            }
            catch (Exception ex)
            {
                // Catch any error that occurs
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Determine Status Code based on exception type
            context.Response.StatusCode = exception switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Forbidden, // 403
                KeyNotFoundException => (int)HttpStatusCode.NotFound,         // 404
                ArgumentException => (int)HttpStatusCode.BadRequest,          // 400
                _ => (int)HttpStatusCode.InternalServerError                  // 500
            };

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = exception.Message,
                // Show details only in Development environment
                details = _env.IsDevelopment() ? exception.StackTrace?.ToString() : null
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}