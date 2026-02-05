namespace WebApi.Infrastructure.Models.Storage;

public class Progress : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int ThemeId { get; set; }
    public Theme Theme { get; set; } = null!;
    
    public int Level { get; set; }
    public double AmountToLevelUp { get; set; }
}