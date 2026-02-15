namespace WebApi.Infrastructure.Models.DTO;

public class Progress
{
    public int UserId { get; set; }
    
    public int ThemeId { get; set; }
    
    public int Level { get; set; }
    public double AmountToLevelUp { get; set; }
}