namespace WebApi.Infrastructure.Models.Storage;

public class TestTask : BaseEntity
{
    public int TaskForTestId { get; set; }
    public TaskForTest TaskForTest { get; set; } = null!;
    
    public int TestId { get; set; }
    public Test Test { get; set; } = null!;
}