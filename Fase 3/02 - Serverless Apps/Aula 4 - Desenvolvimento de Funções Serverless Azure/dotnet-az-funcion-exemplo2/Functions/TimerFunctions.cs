using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TaskManagement.Functions.Interfaces;

namespace TaskManagement.Functions.Functions
{
    public class TimerFunctions
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TimerFunctions> _logger;

        public TimerFunctions(ITaskService taskService, ILogger<TimerFunctions> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        [Function("TimerCleanupFunction")]
        public async Task TimerCleanupFunction([TimerTrigger("0 0 2 * * *")] TimerInfo timerInfo)
        {
            _logger.LogInformation("Iniciando limpeza automática de tarefas antigas às {Time}", DateTime.UtcNow);

            try
            {
                var deletedCount = await _taskService.CleanupOldTasksAsync(30); // Remove tarefas com mais de 30 dias
                
                _logger.LogInformation("Limpeza concluída. {DeletedCount} tarefas antigas foram removidas", deletedCount);
                
                if (deletedCount > 0)
                {
                    _logger.LogInformation("Detalhes da limpeza: Tarefas removidas foram criadas há mais de 30 dias e estavam com status 'Completed' ou 'Cancelled'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante a limpeza automática de tarefas");
                throw;
            }
        }

        [Function("DailyTaskSummary")]
        public async Task DailyTaskSummary([TimerTrigger("0 0 8 * * *")] TimerInfo timerInfo)
        {
            _logger.LogInformation("Gerando resumo diário de tarefas às {Time}", DateTime.UtcNow);

            try
            {
                var allTasks = await _taskService.GetAllTasksAsync();
                var tasksList = allTasks.ToList();

                var summary = new
                {
                    Total = tasksList.Count,
                    Pending = tasksList.Count(t => t.Status == Models.TaskStatus.Pending),
                    InProgress = tasksList.Count(t => t.Status == Models.TaskStatus.InProgress),
                    Completed = tasksList.Count(t => t.Status == Models.TaskStatus.Completed),
                    Cancelled = tasksList.Count(t => t.Status == Models.TaskStatus.Cancelled),
                    CompletedToday = tasksList.Count(t => t.CompletedAt?.Date == DateTime.UtcNow.Date)
                };

                _logger.LogInformation("Resumo diário de tarefas: {@Summary}", summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar resumo diário de tarefas");
                throw;
            }
        }
    }
}
