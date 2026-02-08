namespace WebApi.Infrastructure.Models.DTO;

public class CompletedTask
{
    public int UserId { get; set; }
    
    public int TaskId { get; set; }
    
    public bool? IsCorrect { get; set; }
    public DateTime CompletedAt { get; set; }

}