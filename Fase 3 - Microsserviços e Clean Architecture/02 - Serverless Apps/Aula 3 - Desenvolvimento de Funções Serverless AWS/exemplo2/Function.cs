using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace LambdaTestProject;

public class Function
{
    /// <summary>
    /// Uma função Lambda simples que processa requisições HTTP
    /// </summary>
    /// <param name="request">O evento de requisição do API Gateway</param>
    /// <param name="context">O contexto da execução Lambda</param>
    /// <returns>A resposta HTTP</returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var path = request.Path ?? "/";
        
        context.Logger.LogInformation($"Processando requisição: {request.HttpMethod} {path}");
        context.Logger.LogInformation($"Headers recebidos: {string.Join(", ", request.Headers?.Keys ?? new string[0])}");
        context.Logger.LogInformation($"Path recebido: {path}");
        context.Logger.LogInformation($"Resource: {request.Resource}");
        
        try
        {
            // Simular diferentes fluxos baseados no path da requisição
            var response = path switch
            {
                "/health" => await ProcessHealthCheck(context),
                "/process-order" => await ProcessOrder(request, context),
                "/get-data" => await GetData(request, context),
                "/" => await ProcessHealthCheck(context), // Root também vai para health check
                _ => CreateErrorResponse($"Endpoint não encontrado: {path}", HttpStatusCode.NotFound)
            };

            context.Logger.LogInformation($"Resposta enviada com status: {response.StatusCode}");
            return response;
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Erro durante processamento: {ex.Message}");
            context.Logger.LogError($"Stack trace: {ex.StackTrace}");
            return CreateErrorResponse("Erro interno do servidor", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Endpoint de health check
    /// </summary>
    private Task<APIGatewayProxyResponse> ProcessHealthCheck(ILambdaContext context)
    {
        context.Logger.LogInformation("Health check executado");
        
        var healthData = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "development"
        };

        return Task.FromResult(CreateSuccessResponse(healthData));
    }

    /// <summary>
    /// Simula processamento de um pedido
    /// </summary>
    private async Task<APIGatewayProxyResponse> ProcessOrder(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("Processando pedido");

        // Simular parsing do body da requisição
        if (string.IsNullOrEmpty(request.Body))
        {
            return CreateErrorResponse("Body da requisição é obrigatório", HttpStatusCode.BadRequest);
        }

        try
        {
            var orderData = JsonSerializer.Deserialize<OrderRequest>(request.Body);
            
            // Simular validação de dados
            if (string.IsNullOrEmpty(orderData.ProductId) || orderData.Quantity <= 0)
            {
                return CreateErrorResponse("Dados do pedido inválidos", HttpStatusCode.BadRequest);
            }

            // Simular processamento assíncrono
            await Task.Delay(100); // Simula processamento

            // Simular cálculo de preço
            var totalPrice = orderData.Quantity * 29.99m; // Preço fixo para teste

            var processedOrder = new
            {
                orderId = Guid.NewGuid().ToString(),
                productId = orderData.ProductId,
                quantity = orderData.Quantity,
                totalPrice = totalPrice,
                status = "processed",
                processedAt = DateTime.UtcNow
            };

            context.Logger.LogInformation($"Pedido processado: {processedOrder.orderId}");
            return CreateSuccessResponse(processedOrder);
        }
        catch (JsonException)
        {
            return CreateErrorResponse("JSON inválido no body da requisição", HttpStatusCode.BadRequest);
        }
    }

    /// <summary>
    /// Endpoint para obter dados de exemplo
    /// </summary>
    private async Task<APIGatewayProxyResponse> GetData(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("Obtendo dados de exemplo");

        // Simular busca de dados (poderia ser DynamoDB, S3, etc.)
        await Task.Delay(50); // Simula latência de banco de dados

        var sampleData = new
        {
            items = new[]
            {
                new { id = 1, name = "Produto A", price = 29.99, category = "eletrônicos" },
                new { id = 2, name = "Produto B", price = 19.99, category = "livros" },
                new { id = 3, name = "Produto C", price = 39.99, category = "roupas" }
            },
            totalItems = 3,
            lastUpdated = DateTime.UtcNow
        };

        return CreateSuccessResponse(sampleData);
    }

    /// <summary>
    /// Cria uma resposta de sucesso padrão
    /// </summary>
    private APIGatewayProxyResponse CreateSuccessResponse(object data)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" }
            },
            Body = JsonSerializer.Serialize(data, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            })
        };
    }

    /// <summary>
    /// Cria uma resposta de erro padrão
    /// </summary>
    private APIGatewayProxyResponse CreateErrorResponse(string message, HttpStatusCode statusCode)
    {
        var errorData = new
        {
            error = message,
            timestamp = DateTime.UtcNow
        };

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)statusCode,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" }
            },
            Body = JsonSerializer.Serialize(errorData, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            })
        };
    }
}

/// <summary>
/// Classe para representar dados de pedido
/// </summary>
public class OrderRequest
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
}
