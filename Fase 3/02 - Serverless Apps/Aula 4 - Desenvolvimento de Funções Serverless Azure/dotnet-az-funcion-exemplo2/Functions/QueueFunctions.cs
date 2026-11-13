using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskManagement.Functions.Interfaces;
using TaskManagement.Functions.Models;

namespace TaskManagement.Functions.Functions
{
    public class QueueFunctions
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<QueueFunctions> _logger;

        public QueueFunctions(ITaskService taskService, ILogger<QueueFunctions> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        [Function("ProcessTaskFunction")]
        public async Task ProcessTaskFunction(
            [QueueTrigger("task-processing-queue", Connection = "AzureWebJobsStorage")] string queueItem)
        {
            _logger.LogInformation("Processando mensagem da fila: {QueueItem}", queueItem);

            try
            {
                var taskMessage = JsonSerializer.Deserialize<TaskQueueMessage>(queueItem);
                
                if (taskMessage == null)
                {
                    _logger.LogWarning("Mensagem da fila inválida: {QueueItem}", queueItem);
                    return;
                }

                switch (taskMessage.Action.ToLowerInvariant())
                {
                    case "notify_assignment":
                        await ProcessTaskAssignmentNotification(taskMessage);
                        break;
                    
                    case "auto_complete":
                        await ProcessAutoCompletion(taskMessage);
                        break;
                    
                    case "priority_escalation":
                        await ProcessPriorityEscalation(taskMessage);
                        break;
                    
                    default:
                        _logger.LogWarning("Ação desconhecida na mensagem da fila: {Action}", taskMessage.Action);
                        break;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro ao deserializar mensagem da fila: {QueueItem}", queueItem);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem da fila");
                throw;
            }
        }

        private async Task ProcessTaskAssignmentNotification(TaskQueueMessage message)
        {
            _logger.LogInformation("Processando notificação de atribuição para tarefa {TaskId}", message.TaskId);
            
            var task = await _taskService.GetTaskByIdAsync(message.TaskId);
            if (task == null)
            {
                _logger.LogWarning("Tarefa {TaskId} não encontrada para notificação", message.TaskId);
                return;
            }

            // Simular envio de notificação (email, SMS, etc.)
            _logger.LogInformation("Notificação enviada para {AssignedTo} sobre a tarefa '{TaskTitle}'", 
                task.AssignedTo, task.Title);
            
            // Aqui você implementaria a lógica real de envio de notificação
            await Task.Delay(100); // Simular processamento
        }

        private async Task ProcessAutoCompletion(TaskQueueMessage message)
        {
            _logger.LogInformation("Processando auto-conclusão para tarefa {TaskId}", message.TaskId);
            
            var task = await _taskService.GetTaskByIdAsync(message.TaskId);
            if (task == null)
            {
                _logger.LogWarning("Tarefa {TaskId} não encontrada para auto-conclusão", message.TaskId);
                return;
            }

            if (task.Status == Models.TaskStatus.InProgress)
            {
                var updateDto = new Models.DTOs.UpdateTaskDto
                {
                    Status = Models.TaskStatus.Completed
                };

                await _taskService.UpdateTaskAsync(message.TaskId, updateDto);
                _logger.LogInformation("Tarefa {TaskId} foi automaticamente marcada como concluída", message.TaskId);
            }
        }

        private async Task ProcessPriorityEscalation(TaskQueueMessage message)
        {
            _logger.LogInformation("Processando escalação de prioridade para tarefa {TaskId}", message.TaskId);
            
            var task = await _taskService.GetTaskByIdAsync(message.TaskId);
            if (task == null)
            {
                _logger.LogWarning("Tarefa {TaskId} não encontrada para escalação", message.TaskId);
                return;
            }

            // Simular lógica de escalação
            if (task.Status == Models.TaskStatus.Pending && 
                task.CreatedAt < DateTime.UtcNow.AddDays(-7))
            {
                _logger.LogWarning("Tarefa {TaskId} '{TaskTitle}' está pendente há mais de 7 dias. Escalação necessária!", 
                    message.TaskId, task.Title);
                
                // Aqui você implementaria a lógica de escalação (notificar gerente, etc.)
                await Task.Delay(100); // Simular processamento
            }
        }
    }

    public class TaskQueueMessage
    {
        public string TaskId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
