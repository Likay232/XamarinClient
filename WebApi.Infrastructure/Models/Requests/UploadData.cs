using CompletedTask = WebApi.Infrastructure.Models.DTO.CompletedTask;
using Progress = WebApi.Infrastructure.Models.DTO.Progress;

namespace WebApi.Infrastructure.Models.Requests;

public class UploadData
{
    public List<CompletedTask> CompletedTasks { get; set; } = [];
    public List<Progress> Progresses { get; set; } = [];
}