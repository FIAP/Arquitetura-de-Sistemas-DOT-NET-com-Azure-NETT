namespace TaskManagement.Functions.Models.DTOs
{
    public class TaskResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }
}
