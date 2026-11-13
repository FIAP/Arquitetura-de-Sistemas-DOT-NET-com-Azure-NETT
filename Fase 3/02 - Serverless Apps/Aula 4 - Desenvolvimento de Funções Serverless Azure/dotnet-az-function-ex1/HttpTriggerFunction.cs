using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzureFunctionExample
{
    public class HttpTriggerFunction
    {
        private readonly ILogger _logger;

        public HttpTriggerFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpTriggerFunction>();
        }

        [Function("HttpTriggerExample")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Azure Function HTTP trigger processando uma requisição.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            string? name = null;
            var method = req.Method.ToUpper();

            // Processar requisições GET
            if (method == "GET")
            {
                // Tentar obter 'name' da query string
                var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                name = query["name"];

                object responseData;
                if (name != null)
                {
                    responseData = new
                    {
                        message = $"Olá, {name}! Esta é uma Azure Function .NET com GET.",
                        method = "GET",
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        framework = ".NET 8"
                    };
                }
                else
                {
                    responseData = new
                    {
                        message = "Olá! Esta é uma Azure Function .NET simples. Use ?name=SeuNome para personalizar.",
                        method = "GET",
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        framework = ".NET 8",
                        exemplo = "Tente: ?name=João"
                    };
                }

                await response.WriteStringAsync(JsonSerializer.Serialize(responseData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));
            }
            // Processar requisições POST
            else if (method == "POST")
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                
                try
                {
                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                        if (jsonData != null && jsonData.ContainsKey("name"))
                        {
                            name = jsonData["name"]?.ToString();
                        }

                        object responseData;
                        if (name != null)
                        {
                            responseData = new
                            {
                                message = $"Dados recebidos via POST! Olá, {name}!",
                                method = "POST",
                                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                                framework = ".NET 8",
                                receivedData = jsonData
                            };
                        }
                        else
                        {
                            responseData = new
                            {
                                message = "Dados recebidos via POST! Envie JSON com 'name' para personalizar.",
                                method = "POST",
                                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                                framework = ".NET 8",
                                receivedData = jsonData,
                                exemplo = new { name = "João", idade = 30 }
                            };
                        }

                        await response.WriteStringAsync(JsonSerializer.Serialize(responseData, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        }));
                    }
                    else
                    {
                        var responseData = new
                        {
                            message = "Corpo da requisição vazio. Envie JSON com dados.",
                            method = "POST",
                            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            framework = ".NET 8",
                            exemplo = new { name = "João", idade = 30 }
                        };

                        await response.WriteStringAsync(JsonSerializer.Serialize(responseData, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        }));
                    }
                }
                catch (JsonException)
                {
                    response = req.CreateResponse(HttpStatusCode.BadRequest);
                    response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                    
                    var errorData = new
                    {
                        error = "JSON inválido no corpo da requisição",
                        method = "POST",
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    };

                    await response.WriteStringAsync(JsonSerializer.Serialize(errorData, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    }));
                }
            }
            else
            {
                response = req.CreateResponse(HttpStatusCode.MethodNotAllowed);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                
                var errorData = new
                {
                    error = $"Método {method} não suportado",
                    supportedMethods = new[] { "GET", "POST" },
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(errorData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));
            }

            return response;
        }
    }
}
