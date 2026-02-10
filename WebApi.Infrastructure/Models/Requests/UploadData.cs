using WebApi.Infrastructure.Models.Storage;
using CompletedTask = WebApi.Infrastructure.Models.DTO.CompletedTask;

namespace WebApi.Infrastructure.Models.Requests;

public class UploadData
{
    public List<CompletedTask> CompletedTasks { get; set; }
    public List<Progress> Progresses { get; set; }
}