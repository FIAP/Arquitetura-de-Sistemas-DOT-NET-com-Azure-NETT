using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TaskManagement.Functions.Interfaces;
using TaskManagement.Functions.Models;
using TaskStatus = TaskManagement.Functions.Models.TaskStatus;

namespace TaskManagement.Functions.Functions
{
    public class HealthCheckFunction
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<HealthCheckFunction> _logger;

        public HealthCheckFunction(ITaskService taskService, ILogger<HealthCheckFunction> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        [Function("HealthCheck")]
        public async Task<HttpResponseData> HealthCheck(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
        {
            _logger.LogInformation("Health check requisitado às {Time}", DateTime.UtcNow);

            var healthStatus = new HealthStatus();

            try
            {
                // Verificar se o serviço de tarefas está funcionando
                var tasks = await _taskService.GetAllTasksAsync();
                var taskCount = tasks.Count();
                
                healthStatus.Database = "Healthy";
                healthStatus.TaskService = "Healthy";
                healthStatus.TaskCount = taskCount;
                healthStatus.Status = "Healthy";
                healthStatus.Message = "Todos os serviços estão funcionando corretamente";

                _logger.LogInformation("Health check concluído com sucesso. Total de tarefas: {TaskCount}", taskCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante health check");
                
                healthStatus.Database = "Unhealthy";
                healthStatus.TaskService = "Unhealthy";
                healthStatus.Status = "Unhealthy";
                healthStatus.Message = $"Erro nos serviços: {ex.Message}";
                healthStatus.Error = ex.Message;
            }

            var statusCode = healthStatus.Status == "Healthy" ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable;
            return await CreateJsonResponse(req, healthStatus, statusCode);
        }

        [Function("DetailedHealthCheck")]
        public async Task<HttpResponseData> DetailedHealthCheck(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health/detailed")] HttpRequestData req)
        {
            _logger.LogInformation("Health check detalhado requisitado às {Time}", DateTime.UtcNow);

            var detailedHealth = new DetailedHealthStatus
            {
                Timestamp = DateTime.UtcNow,
                Environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Development",
                Version = "1.0.0"
            };

            try
            {
                // Verificar TaskService
                var allTasks = await _taskService.GetAllTasksAsync();
                var tasksList = allTasks.ToList();

                detailedHealth.Services.Add("TaskService", new ServiceHealth
                {
                    Status = "Healthy",
                    ResponseTime = await MeasureResponseTime(async () => await _taskService.GetAllTasksAsync()),
                    Details = $"Total de tarefas: {tasksList.Count}"
                });

                // Verificar Repository
                detailedHealth.Services.Add("Repository", new ServiceHealth
                {
                    Status = "Healthy",
                    ResponseTime = await MeasureResponseTime(async () => await _taskService.GetAllTasksAsync()),
                    Details = "Repository em memória funcionando"
                });

                // Estatísticas das tarefas
                var stats = new
                {
                    Total = tasksList.Count,
                    Pending = tasksList.Count(t => t.Status == TaskStatus.Pending),
                    InProgress = tasksList.Count(t => t.Status == TaskStatus.InProgress),
                    Completed = tasksList.Count(t => t.Status == TaskStatus.Completed),
                    Cancelled = tasksList.Count(t => t.Status == TaskStatus.Cancelled)
                };

                detailedHealth.Statistics = stats;
                detailedHealth.OverallStatus = "Healthy";
                detailedHealth.Message = "Todos os serviços estão funcionando corretamente";

                _logger.LogInformation("Health check detalhado concluído com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante health check detalhado");
                
                detailedHealth.Services.Add("Error", new ServiceHealth
                {
                    Status = "Unhealthy",
                    ResponseTime = 0,
                    Details = ex.Message
                });

                detailedHealth.OverallStatus = "Unhealthy";
                detailedHealth.Message = $"Erro nos serviços: {ex.Message}";
                detailedHealth.Error = ex.Message;
            }

            var statusCode = detailedHealth.OverallStatus == "Healthy" ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable;
            return await CreateJsonResponse(req, detailedHealth, statusCode);
        }

        private static async Task<long> MeasureResponseTime(Func<Task> operation)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await operation();
                return stopwatch.ElapsedMilliseconds;
            }
            catch
            {
                return stopwatch.ElapsedMilliseconds;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        private static async Task<HttpResponseData> CreateJsonResponse<T>(HttpRequestData req, T data, HttpStatusCode statusCode)
        {
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            
            await response.WriteStringAsync(json);
            return response;
        }
    }

    public class HealthStatus
    {
        public string Status { get; set; } = "Unknown";
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Database { get; set; } = "Unknown";
        public string TaskService { get; set; } = "Unknown";
        public int TaskCount { get; set; }
        public string? Error { get; set; }
    }

    public class DetailedHealthStatus
    {
        public string OverallStatus { get; set; } = "Unknown";
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Environment { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public Dictionary<string, ServiceHealth> Services { get; set; } = new();
        public object? Statistics { get; set; }
        public string? Error { get; set; }
    }

    public class ServiceHealth
    {
        public string Status { get; set; } = "Unknown";
        public long ResponseTime { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}
