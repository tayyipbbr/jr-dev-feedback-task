using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MassTransit; 
using Microsoft.AspNetCore.Mvc;

namespace FeedbackApp.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
                _logger.LogError(ex, "Uygulamada beklenmedik bir hata oluştu: {ErrorMessage}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";
            var statusCode = HttpStatusCode.InternalServerError;
            string title = "Sunucuda bir hata oluştu.";
            string detail = "İsteğiniz işlenirken beklenmedik bir durumla karşılaşıldı. Lütfen daha sonra tekrar deneyin.";

            switch (exception)
            {
                case MassTransitException mtEx:
                    statusCode = HttpStatusCode.ServiceUnavailable; // 503
                    title = "Mesajlaşma servisi hatası.";
                    detail = "Mesaj kuyruğuna erişimde geçici bir sorun yaşanıyor. Lütfen daha sonra tekrar deneyin.";
                    break;
                case TimeoutException timeoutEx:
                    statusCode = HttpStatusCode.GatewayTimeout; // 504
                    title = "İşlem zaman aşımına uğradı.";
                    detail = "İsteğiniz beklenenden uzun sürdü ve zaman aşımına uğradı. Lütfen daha sonra tekrar deneyin.";
                    break;
                default:
                    // diğer tüm bilinmeyen hatalar için 500
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            var jsonResponse = JsonSerializer.Serialize(problemDetails);
            return context.Response.WriteAsync(jsonResponse);
        }
    }

    // middleware'i IApplicationBuilder'a eklemek için metot
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}