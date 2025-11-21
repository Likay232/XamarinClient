namespace WebApi.Infrastructure.Models.DTO;

public class LessonDto
{
    public int ThemeId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Link { get; set; }
}