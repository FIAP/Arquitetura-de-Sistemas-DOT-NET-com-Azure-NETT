using TaskManagement.Functions.Models;
using TaskManagement.Functions.Models.DTOs;
using TaskStatus = TaskManagement.Functions.Models.TaskStatus;

namespace TaskManagement.Functions.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync();
        Task<TaskResponseDto?> GetTaskByIdAsync(string id);
        Task<IEnumerable<TaskResponseDto>> GetTasksByStatusAsync(TaskStatus status);
        Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto);
        Task<TaskResponseDto?> UpdateTaskAsync(string id, UpdateTaskDto updateTaskDto);
        Task<bool> DeleteTaskAsync(string id);
        Task<int> CleanupOldTasksAsync(int daysOld = 30);
    }
}
