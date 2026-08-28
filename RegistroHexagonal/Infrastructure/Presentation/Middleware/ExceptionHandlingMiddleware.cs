using System.Net;
using System.Text.Json;
using RegistroHexagonal.Application.DTOs;
using RegistroHexagonal.Domain.Exceptions;

namespace RegistroHexagonal.Infrastructure.Presentation.Middleware
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

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, mensaje, logComoAdvertencia) = ex switch
            {
                NotFoundException => (HttpStatusCode.NotFound, ex.Message, true),
                BusinessRuleException => (HttpStatusCode.BadRequest, ex.Message, true),
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message, true),
                _ => (HttpStatusCode.InternalServerError, "Ocurrió un error inesperado. Intente nuevamente más tarde.", false)
            };

            if (logComoAdvertencia)
            {
                _logger.LogWarning(ex,
                    "Error controlado en {Metodo} {Ruta}: {Mensaje}",
                    context.Request.Method, context.Request.Path, ex.Message);
            }
            else
            {
                _logger.LogError(ex,
                    "Error no controlado en {Metodo} {Ruta} | TraceId: {TraceId}",
                    context.Request.Method, context.Request.Path, context.TraceIdentifier);
            }

            var respuesta = new ApiResponse<object>(false, mensaje, null);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(respuesta));
        }
    }
}
