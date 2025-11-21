namespace WebApi.Infrastructure.Models.Storage;

public class TestTask : BaseEntity
{
    public int TaskId { get; set; }
    public Task Task { get; set; } = null!;
    
    public int TestId { get; set; }
    public Test Test { get; set; } = null!;
}