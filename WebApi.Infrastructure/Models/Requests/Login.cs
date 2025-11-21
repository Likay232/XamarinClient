namespace WebApi.Infrastructure.Models.Requests;

public class Login
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}