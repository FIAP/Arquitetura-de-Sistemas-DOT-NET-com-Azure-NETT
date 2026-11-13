using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TaskManagement.Functions.Models;

namespace TaskManagement.Functions.Middleware
{
    public class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado na função {FunctionName}", context.FunctionDefinition.Name);
                
                // Apenas para HTTP triggers
                if (context.FunctionDefinition.InputBindings.Any(b => b.Value.Type == "httpTrigger"))
                {
                    await HandleHttpException(context, ex);
                }
            }
        }

        private static async Task HandleHttpException(FunctionContext context, Exception ex)
        {
            var httpResponseData = context.GetHttpResponseData();
            if (httpResponseData != null)
            {
                var response = ex switch
                {
                    ArgumentException => ApiResponse.Failure("Parâmetros inválidos", new List<string> { ex.Message }),
                    KeyNotFoundException => ApiResponse.Failure("Recurso não encontrado"),
                    UnauthorizedAccessException => ApiResponse.Failure("Acesso não autorizado"),
                    _ => ApiResponse.Failure("Erro interno do servidor")
                };

                httpResponseData.StatusCode = ex switch
                {
                    ArgumentException => HttpStatusCode.BadRequest,
                    KeyNotFoundException => HttpStatusCode.NotFound,
                    UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                    _ => HttpStatusCode.InternalServerError
                };

                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                httpResponseData.Headers.Add("Content-Type", "application/json");
                using var writer = new StreamWriter(httpResponseData.Body);
                await writer.WriteAsync(jsonResponse);
            }
        }
    }
}
