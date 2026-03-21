namespace TaskFlow.Api.Models;

public class TodoItem
{
    public int Id { get; set; }             // Primary key — EF detects this automatically
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } // nullable — the ? means it's optional
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; } // null until the task is done

    public int UserId {get; set;}
    public User User { get; set; } = null!;
}
