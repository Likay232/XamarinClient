namespace WebApi.Infrastructure.Models.Requests;

public class GetTasks
{
    public int UserId { get; set; }
    public int ThemeId { get; set; }
}