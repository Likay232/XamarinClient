namespace WebApi.Infrastructure.Models.Requests;

public class GenerateTest
{
    public Dictionary<int, int> DesiredTasksAmount { get; set; } = new();
    public int UserId { get; set; }
}