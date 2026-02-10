namespace WebApi.Infrastructure.Models.Storage;

public class UserToken : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }
    public string RefreshToken { get; set; } =  string.Empty;
    public DateTime ExpiresAt { get; set; } =  DateTime.UtcNow.AddDays(30);
}