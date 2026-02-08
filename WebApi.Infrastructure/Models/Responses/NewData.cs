using WebApi.Infrastructure.Models.Storage;
using CompletedTask = WebApi.Infrastructure.Models.Storage.CompletedTask;

namespace WebApi.Infrastructure.Models.Responses;

public class NewData
{
    public List<CompletedTask> CompletedTasks { get; set; } = [];
    public List<Lesson> Lessons { get; set; } = [];
    public List<Progress> Progresses { get; set; } = [];
    public List<Storage.Task> Tasks { get; set; } = [];
    public List<Theme> Themes { get; set; } = [];
    public List<User> Users { get; set; } = [];
}