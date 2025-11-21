namespace WebApi.Infrastructure.Models.Requests;

public class CreateTest
{
    public string Title { get; set; } = string.Empty;

    public List<int> TaskIds { get; set; } = new();
}