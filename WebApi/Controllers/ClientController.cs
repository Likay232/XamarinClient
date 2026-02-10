using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Enums;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Responses;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ClientController(ClientService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NewData>> GetNewData(DateTime? lastExchange, int userId)
    {
        try
        {
            return StatusCode(200, await service.GetNewData(lastExchange ?? DateTime.MinValue, userId));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<bool>> UploadData(UploadData request)
    {
        try
        {
            var tasks = new List<Task<bool>>
            {
                service.UploadData(request.CompletedTasks),
                service.UploadData(request.Progresses)
            };

            await Task.WhenAll(tasks);
            
            return StatusCode(200);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<ProfileInfo>> GetProfileInfo([FromQuery] int userId)
    {
        try
        {
            var profileInfo = await service.GetProfileInfo(userId);
        
            return StatusCode(200,  profileInfo);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<bool>> RegisterDevice(RegisterDevice request)
    {
        try
        {
            return StatusCode(200, await service.RegisterDevice(request));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ThemeDto>>> GetThemes()
    {
        try
        {
            var themes = await service.GetThemes();

            return StatusCode(200, themes);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<List<TaskForClientDto>>> GetTasksForTheme(GetTasks request)
    {
        try
        {
            var tasks = await service.GetTasksForTheme(request);

            return StatusCode(200, tasks);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<LessonDto>>> GetLessonsForTheme(int themeId)
    {
        try
        {
            var lessons = await service.GetLessonsForTheme(themeId);

            return StatusCode(200, lessons);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<TestDto>>> GetTests()
    {
        try
        {
            var tests = await service.GetTests();

            return StatusCode(200, tests);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<TaskForClientDto>>> GetTest(int testId)
    {
        try
        {
            var tasks = await service.GetTest(testId);

            return StatusCode(200, tasks);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<TaskDto>> GetRandomTask()
    {
        try
        {
            var randomTask = await service.GetRandomTask();

            return StatusCode(200, randomTask);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<bool>> ChangePassword(ChangePasswordClient request)
    {
        try
        {
            return StatusCode(200, await service.ChangePassword(request));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<TestForClientDto>> GenerateTest(TestTypes testType, int themeId, int userId)
    {
        try
        {
            return StatusCode(200, await service.GenerateTest(testType, themeId, userId));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult> GetTaskAmount()
    {
        try
        {
            return StatusCode(200, await service.GetTaskAmount());
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<TestForClientDto>> SaveAnswer(int userId, int taskId, bool isCorrect)
    {
        try
        {
            await service.SaveAnswer(userId, taskId, isCorrect);
            
            return StatusCode(200, true);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<TestForClientDto>> SaveAnswers(SaveAnswers request)
    {
        try
        {
            await service.SaveAnswers(request);
            
            return StatusCode(200, true);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<byte[]>> GetFileBytes(string fileName)
    {
        try
        {
            return StatusCode(200, await service.GetFileBytes(fileName));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ThemesStatistic>>> GetStatisticForTheme()
    {
        try
        {
            var userIdStr = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
                throw new Exception("No user claim found");
        
            int.TryParse(userIdStr, out var userId);

            var statistic = await service.GetStatisticForThemes(userId);
            
            return StatusCode(200, statistic);
        }
        catch (Exception exception)
        {
            return StatusCode(500, exception.Message);
        }
    }
}