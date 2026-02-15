using WebApi.Infrastructure.Models.DTO;

namespace WebApi.Infrastructure.Models.Requests;

public class SaveAnswers
{
    public int UserId { get; set; }
    public List<UserAnswer> UserAnswers { get; set; } = [];
    public bool IsPassed { get; set; } = false;
    public int MistakesCount { get; set; }
}