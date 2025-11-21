namespace WebApi.Infrastructure.Models.Requests;

public class CreateTheme
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}