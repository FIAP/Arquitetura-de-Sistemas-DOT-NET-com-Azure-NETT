namespace TaskManagement.Functions.Models.DTOs
{
    public class UpdateTaskDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TaskStatus? Status { get; set; }
        public string? AssignedTo { get; set; }
        public List<string>? Tags { get; set; }
    }
}
