using TaskManagement.Functions.Models;
using TaskStatus = TaskManagement.Functions.Models.TaskStatus;

namespace TaskManagement.Functions.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskModel>> GetAllAsync();
        Task<TaskModel?> GetByIdAsync(string id);
        Task<IEnumerable<TaskModel>> GetByStatusAsync(TaskStatus status);
        Task<TaskModel> CreateAsync(TaskModel task);
        Task<TaskModel?> UpdateAsync(string id, TaskModel task);
        Task<bool> DeleteAsync(string id);
        Task<IEnumerable<TaskModel>> GetOldTasksAsync(DateTime cutoffDate);
        Task<int> DeleteOldTasksAsync(DateTime cutoffDate);
    }
}
