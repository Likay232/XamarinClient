using Microsoft.AspNetCore.Http;

namespace WebApi.Infrastructure.Models.DTO;

public class TaskCreateDto
{
    public int ThemeId { get; set; }
    
    public string Text { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    
    public int DifficultyLevel { get; set; }

    public IFormFile? ImageFile { get; set; }
    public IFormFile? FileData { get; set; }
}