using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using FluentValidation;
using TaskManagement.Functions.Interfaces;
using TaskManagement.Functions.Models;
using TaskManagement.Functions.Models.DTOs;
using TaskStatus = TaskManagement.Functions.Models.TaskStatus;

namespace TaskManagement.Functions.Functions
{
    public class TaskFunctions
    {
        private readonly ITaskService _taskService;
        private readonly IValidator<CreateTaskDto> _createValidator;
        private readonly IValidator<UpdateTaskDto> _updateValidator;
        private readonly ILogger<TaskFunctions> _logger;

        public TaskFunctions(
            ITaskService taskService,
            IValidator<CreateTaskDto> createValidator,
            IValidator<UpdateTaskDto> updateValidator,
            ILogger<TaskFunctions> logger)
        {
            _taskService = taskService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        [Function("GetAllTasks")]
        public async Task<HttpResponseData> GetAllTasks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks")] HttpRequestData req)
        {
            _logger.LogInformation("Buscando todas as tarefas");

            var tasks = await _taskService.GetAllTasksAsync();
            var response = ApiResponse<IEnumerable<TaskResponseDto>>.SuccessResult(tasks, "Tarefas recuperadas com sucesso");

            return await CreateJsonResponse(req, response, HttpStatusCode.OK);
        }

        [Function("GetTaskById")]
        public async Task<HttpResponseData> GetTaskById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks/{id}")] HttpRequestData req,
            string id)
        {
            _logger.LogInformation("Buscando tarefa com ID: {TaskId}", id);

            if (string.IsNullOrEmpty(id))
            {
                var errorResponse = ApiResponse<TaskResponseDto>.FailureResult("ID da tarefa é obrigatório");
                return await CreateJsonResponse(req, errorResponse, HttpStatusCode.BadRequest);
            }

            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                var notFoundResponse = ApiResponse<TaskResponseDto>.FailureResult("Tarefa não encontrada");
                return await CreateJsonResponse(req, notFoundResponse, HttpStatusCode.NotFound);
            }

            var response = ApiResponse<TaskResponseDto>.SuccessResult(task, "Tarefa recuperada com sucesso");
            return await CreateJsonResponse(req, response, HttpStatusCode.OK);
        }

        [Function("CreateTask")]
        public async Task<HttpResponseData> CreateTask(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tasks")] HttpRequestData req)
        {
            _logger.LogInformation("Criando nova tarefa");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var createTaskDto = JsonSerializer.Deserialize<CreateTaskDto>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (createTaskDto == null)
                {
                    var errorResponse = ApiResponse<TaskResponseDto>.FailureResult("Dados da tarefa são obrigatórios");
                    return await CreateJsonResponse(req, errorResponse, HttpStatusCode.BadRequest);
                }

                var validationResult = await _createValidator.ValidateAsync(createTaskDto);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    var validationResponse = ApiResponse<TaskResponseDto>.FailureResult("Dados inválidos", errors);
                    return await CreateJsonResponse(req, validationResponse, HttpStatusCode.BadRequest);
                }

                var createdTask = await _taskService.CreateTaskAsync(createTaskDto);
                var response = ApiResponse<TaskResponseDto>.SuccessResult(createdTask, "Tarefa criada com sucesso");

                return await CreateJsonResponse(req, response, HttpStatusCode.Created);
            }
            catch (JsonException)
            {
                var errorResponse = ApiResponse<TaskResponseDto>.FailureResult("Formato JSON inválido");
                return await CreateJsonResponse(req, errorResponse, HttpStatusCode.BadRequest);
            }
        }

        [Function("UpdateTask")]
        public async Task<HttpResponseData> UpdateTask(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "tasks/{id}")] HttpRequestData req,
            string id)
        {
            _logger.LogInformation("Atualizando tarefa com ID: {TaskId}", id);

            if (string.IsNullOrEmpty(id))
            {
                var errorResponse = ApiResponse<TaskResponseDto>.FailureResult("ID da tarefa é obrigatório");
                return await CreateJsonResponse(req, errorResponse, HttpStatusCode.BadRequest);
            }

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updateTaskDto = JsonSerializer.Deserialize<UpdateTaskDto>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (updateTaskDto == null)
                {
                    var errorResponse = ApiResponse<TaskResponseDto>.FailureResult("Dados para atualização são obrigatórios");
                    return await CreateJsonResponse(req, errorResponse, HttpStatusCode.BadRequest);
                }

                var validationResult = await _updateValidator.ValidateAsync(updateTaskDto);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    var validationResponse = ApiResponse<TaskResponseDto>.FailureResult("Dados inválidos", errors);
                    return await CreateJsonResponse(req, validationResponse, HttpStatusCode.BadRequest);
                }

                var updatedTask = await _taskService.UpdateTaskAsync(id, updateTaskDto);
                if (updatedTask == null)
                {
                    var notFoundResponse = ApiResponse<TaskResponseDto>.FailureResult("Tarefa não encontrada");
                    return await CreateJsonResponse(req, notFoundResponse, HttpStatusCode.NotFound);
                }

                var response = ApiResponse<TaskResponseDto>.SuccessResult(updatedTask, "Tarefa atualizada com sucesso");
                return await CreateJsonResponse(req, response, HttpStatusCode.OK);
            }
            catch (JsonException)
            {
                var errorResponse = ApiResponse<TaskResponseDto>.FailureResult("Formato JSON inválido");
                return await CreateJsonResponse(req, errorResponse, HttpStatusCode.BadRequest);
            }
        }

        [Function("DeleteTask")]
        public async Task<HttpResponseData> DeleteTask(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "tasks/{id}")] HttpRequestData req,
            string id)
        {
            _logger.LogInformation("Deletando tarefa com ID: {TaskId}", id);

            if (string.IsNullOrEmpty(id))
            {
                var errorResponse = ApiResponse.Failure("ID da tarefa é obrigatório");
                return await CreateJsonResponse(req, errorResponse, HttpStatusCode.BadRequest);
            }

            var deleted = await _taskService.DeleteTaskAsync(id);
            if (!deleted)
            {
                var notFoundResponse = ApiResponse.Failure("Tarefa não encontrada");
                return await CreateJsonResponse(req, notFoundResponse, HttpStatusCode.NotFound);
            }

            var response = ApiResponse.Success("Tarefa deletada com sucesso");
            return await CreateJsonResponse(req, response, HttpStatusCode.OK);
        }

        [Function("GetTasksByStatus")]
        public async Task<HttpResponseData> GetTasksByStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks/status/{status}")] HttpRequestData req,
            string status)
        {
            _logger.LogInformation("Buscando tarefas com status: {Status}", status);

            if (!Enum.TryParse<TaskStatus>(status, true, out var taskStatus))
            {
                var errorResponse = ApiResponse<IEnumerable<TaskResponseDto>>.FailureResult("Status inválido. Valores válidos: Pending, InProgress, Completed, Cancelled");
                return await CreateJsonResponse(req, errorResponse, HttpStatusCode.BadRequest);
            }

            var tasks = await _taskService.GetTasksByStatusAsync(taskStatus);
            var response = ApiResponse<IEnumerable<TaskResponseDto>>.SuccessResult(tasks, $"Tarefas com status '{taskStatus}' recuperadas com sucesso");

            return await CreateJsonResponse(req, response, HttpStatusCode.OK);
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
}
