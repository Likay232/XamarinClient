namespace WebApi.Infrastructure.Models.DTO;

public class UserAnswer
{
    public int TaskId { get; set; }
    public bool IsCorrect { get; set; }
    public string Answer { get; set; } = string.Empty;
}