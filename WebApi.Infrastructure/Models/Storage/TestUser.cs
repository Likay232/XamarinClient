namespace WebApi.Infrastructure.Models.Storage;

public class TestUser : BaseEntity
{
    public int TestId { get; set; }
    public Test? Test { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public int MistakesCount { get; set; }
    public bool IsPassed { get; set; }
    public DateTime CompletionDate { get; set; } = DateTime.UtcNow;
}