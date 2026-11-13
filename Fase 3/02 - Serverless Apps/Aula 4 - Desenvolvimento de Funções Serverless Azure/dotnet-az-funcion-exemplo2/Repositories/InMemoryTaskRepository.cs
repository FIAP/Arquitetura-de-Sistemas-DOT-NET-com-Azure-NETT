using System.Collections.Concurrent;
using TaskManagement.Functions.Interfaces;
using TaskManagement.Functions.Models;
using TaskStatus = TaskManagement.Functions.Models.TaskStatus;

namespace TaskManagement.Functions.Repositories
{
    public class InMemoryTaskRepository : ITaskRepository
    {
        private readonly ConcurrentDictionary<string, TaskModel> _tasks = new();

        public Task<IEnumerable<TaskModel>> GetAllAsync()
        {
            return Task.FromResult(_tasks.Values.AsEnumerable());
        }

        public Task<TaskModel?> GetByIdAsync(string id)
        {
            _tasks.TryGetValue(id, out var task);
            return Task.FromResult(task);
        }

        public Task<IEnumerable<TaskModel>> GetByStatusAsync(TaskStatus status)
        {
            var tasks = _tasks.Values.Where(t => t.Status == status);
            return Task.FromResult(tasks);
        }

        public Task<TaskModel> CreateAsync(TaskModel task)
        {
            task.Id = Guid.NewGuid().ToString();
            task.CreatedAt = DateTime.UtcNow;
            _tasks.TryAdd(task.Id, task);
            return Task.FromResult(task);
        }

        public Task<TaskModel?> UpdateAsync(string id, TaskModel task)
        {
            if (!_tasks.TryGetValue(id, out var existingTask))
            {
                return Task.FromResult<TaskModel?>(null);
            }

            // Preservar dados imutáveis
            task.Id = existingTask.Id;
            task.CreatedAt = existingTask.CreatedAt;
            
            // Atualizar CompletedAt se status mudou para Completed
            if (task.Status == TaskStatus.Completed && existingTask.Status != TaskStatus.Completed)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            else if (task.Status != TaskStatus.Completed)
            {
                task.CompletedAt = null;
            }

            _tasks.TryUpdate(id, task, existingTask);
            return Task.FromResult<TaskModel?>(task);
        }

        public Task<bool> DeleteAsync(string id)
        {
            var result = _tasks.TryRemove(id, out _);
            return Task.FromResult(result);
        }

        public Task<IEnumerable<TaskModel>> GetOldTasksAsync(DateTime cutoffDate)
        {
            var oldTasks = _tasks.Values
                .Where(t => t.CreatedAt < cutoffDate && 
                           (t.Status == TaskStatus.Completed || t.Status == TaskStatus.Cancelled));
            return Task.FromResult(oldTasks);
        }

        public Task<int> DeleteOldTasksAsync(DateTime cutoffDate)
        {
            var oldTasks = _tasks.Values
                .Where(t => t.CreatedAt < cutoffDate && 
                           (t.Status == TaskStatus.Completed || t.Status == TaskStatus.Cancelled))
                .ToList();

            var deletedCount = 0;
            foreach (var task in oldTasks)
            {
                if (_tasks.TryRemove(task.Id, out _))
                {
                    deletedCount++;
                }
            }

            return Task.FromResult(deletedCount);
        }
    }
}
