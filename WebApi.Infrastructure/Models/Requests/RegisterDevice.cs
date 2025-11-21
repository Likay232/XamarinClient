namespace WebApi.Infrastructure.Models.Requests;

public class RegisterDevice
{
    public int UserId { get; set; }
    public string DeviceToken { get; set; } = string.Empty;
}