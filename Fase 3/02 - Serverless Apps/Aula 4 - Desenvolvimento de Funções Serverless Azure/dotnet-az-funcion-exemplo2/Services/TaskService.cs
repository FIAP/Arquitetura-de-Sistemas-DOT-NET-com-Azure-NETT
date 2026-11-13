using AutoMapper;
using TaskManagement.Functions.Interfaces;
using TaskManagement.Functions.Models;
using TaskManagement.Functions.Models.DTOs;
using TaskStatus = TaskManagement.Functions.Models.TaskStatus;

namespace TaskManagement.Functions.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;

        public TaskService(ITaskRepository taskRepository, IMapper mapper)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TaskResponseDto>>(tasks);
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(string id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            return task != null ? _mapper.Map<TaskResponseDto>(task) : null;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetTasksByStatusAsync(TaskStatus status)
        {
            var tasks = await _taskRepository.GetByStatusAsync(status);
            return _mapper.Map<IEnumerable<TaskResponseDto>>(tasks);
        }

        public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto)
        {
            var task = _mapper.Map<TaskModel>(createTaskDto);
            var createdTask = await _taskRepository.CreateAsync(task);
            return _mapper.Map<TaskResponseDto>(createdTask);
        }

        public async Task<TaskResponseDto?> UpdateTaskAsync(string id, UpdateTaskDto updateTaskDto)
        {
            var existingTask = await _taskRepository.GetByIdAsync(id);
            if (existingTask == null)
            {
                return null;
            }

            // Aplicar apenas as propriedades que não são nulas
            if (!string.IsNullOrEmpty(updateTaskDto.Title))
                existingTask.Title = updateTaskDto.Title;
            
            if (!string.IsNullOrEmpty(updateTaskDto.Description))
                existingTask.Description = updateTaskDto.Description;
            
            if (updateTaskDto.Status.HasValue)
                existingTask.Status = updateTaskDto.Status.Value;
            
            if (!string.IsNullOrEmpty(updateTaskDto.AssignedTo))
                existingTask.AssignedTo = updateTaskDto.AssignedTo;
            
            if (updateTaskDto.Tags != null)
                existingTask.Tags = updateTaskDto.Tags;

            var updatedTask = await _taskRepository.UpdateAsync(id, existingTask);
            return updatedTask != null ? _mapper.Map<TaskResponseDto>(updatedTask) : null;
        }

        public async Task<bool> DeleteTaskAsync(string id)
        {
            return await _taskRepository.DeleteAsync(id);
        }

        public async Task<int> CleanupOldTasksAsync(int daysOld = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            return await _taskRepository.DeleteOldTasksAsync(cutoffDate);
        }
    }
}
