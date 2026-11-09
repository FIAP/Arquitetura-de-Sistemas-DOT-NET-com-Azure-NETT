using System.Text.Json.Serialization;

namespace TaskManagement.Functions.Models
{
    public class TaskModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }
}
