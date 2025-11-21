namespace WebApi.Infrastructure.Models.Requests;

public class ChangePasswordAdmin
{
    public int UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}