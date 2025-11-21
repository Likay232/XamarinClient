namespace WebApi.Infrastructure.Models.Requests;

public class CheckTask
{
    public int UserId { get; set; }
    public int TaskId { get; set; }
    public string Answer { get; set; } = string.Empty;
}