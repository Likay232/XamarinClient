using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;
using Task = WebApi.Infrastructure.Models.Storage.Task;

namespace WebApi.Services;

public class AdminService(DataComponent component, IWebHostEnvironment env)
{
    public async Task<List<UserDto>> GetUsers()
    {
        return await component.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsBlocked = u.IsBlocked
            })
            .ToListAsync();
    }

    public async Task<User?> GetUser(int userId)
    {
        return await component.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> SwitchBlockState(int userId)
    {
        var userToBlock = await component.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (userToBlock == null)
            throw new Exception("Пользователь не найден.");

        userToBlock.IsBlocked = !userToBlock.IsBlocked;

        return await component.Update(userToBlock);
    }

    public async Task<bool> ChangeUserPassword(ChangePasswordAdmin request)
    {
        var userEntry = await component.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (userEntry == null)
            throw new Exception("Пользователь с заданным Id не найден");

        userEntry.Password = request.NewPassword;

        return await component.Update(userEntry);
    }

    public async Task<List<ThemeDto>> GetThemes()
    {
        return await component.Themes.Select(t => new ThemeDto
        {
            Id = t.Id,
            Description = t.Description,
            Title = t.Title
        }).ToListAsync();
    }

    public async Task<List<TaskDto>> GetTasksForTheme(int themeId)
    {
        return await component.Tasks
            .Where(t => t.ThemeId == themeId)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                ThemeId = t.ThemeId,
                Text = t.Text,
                CorrectAnswer = t.CorrectAnswer,
                DifficultyLevel = t.DifficultyLevel,
                FilePath = t.FilePath,
                ImageData = t.ImageData
            })
            .ToListAsync();
    }

    public async Task<TaskDto?> GetTaskById(int taskId)
    {
        var task = await component.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
        return task == null
            ? null
            : new TaskDto
            {
                Id = task.Id,
                Text = task.Text,
                CorrectAnswer = task.CorrectAnswer,
                DifficultyLevel = task.DifficultyLevel,
                ThemeId = task.ThemeId,
                ImageData = task.ImageData,
                FilePath = task.FilePath
            };
    }

    public async Task<bool> CreateNewTheme(CreateTheme request)
    {
        var newTheme = new Theme
        {
            Title = request.Title,
            Description = request.Description,
        };

        return await component.Insert(newTheme);
    }

    public async Task<bool> AddTaskForTheme(TaskDto taskToAdd)
    {
        if (!component.Themes.Any(t => t.Id == taskToAdd.ThemeId))
            throw new Exception("Тема с таким Id не найдена.");
        
        var newTask = new Task
        {
            ThemeId = taskToAdd.ThemeId,
            Text = taskToAdd.Text,
            CorrectAnswer = taskToAdd.CorrectAnswer,
            DifficultyLevel = taskToAdd.DifficultyLevel,
            ImageData = taskToAdd.ImageData,
            FilePath = taskToAdd.FilePath,
        };

        return await component.Insert(newTask);
    }

    public async Task<bool> EditTaskForTheme(TaskDto updatedTask)
    {
        if (!component.Themes.Any(t => t.Id == updatedTask.ThemeId))
            throw new Exception("Тема с таким Id не найдена.");

        var taskToEdit = component.Tasks.FirstOrDefault(t => t.Id == updatedTask.Id);

        if (taskToEdit == null)
            throw new Exception("Задание с таким Id не найдено.");

        taskToEdit.Text = updatedTask.Text;
        taskToEdit.CorrectAnswer = updatedTask.CorrectAnswer;
        taskToEdit.DifficultyLevel = updatedTask.DifficultyLevel;
        taskToEdit.ImageData = updatedTask.ImageData;
        taskToEdit.FilePath = updatedTask.FilePath;
        taskToEdit.ThemeId = updatedTask.ThemeId;

        return await component.Update(taskToEdit);
    }

    public async Task<bool> DeleteTaskForTheme(int taskId)
    {
        return await component.Delete<Task>(taskId);
    }

    public async Task<bool> AddLessonForTheme(LessonDto lessonToAdd)
    {
        if (!component.Themes.Any(t => t.Id == lessonToAdd.ThemeId))
            throw new Exception("Тема с таким Id не найдена.");

        var newLesson = new Lesson
        {
            ThemeId = lessonToAdd.ThemeId,
            Text = lessonToAdd.Text,
            Link = lessonToAdd.Link
        };

        return await component.Insert(newLesson);
    }

    public async Task<List<TaskDto>> GetTasks()
    {
        return await component.Tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Text = t.Text,
                CorrectAnswer = t.CorrectAnswer,
                DifficultyLevel = t.DifficultyLevel,
                ImageData = t.ImageData,
                FilePath = t.FilePath,
            })
            .ToListAsync();
    }

    public async Task<string> CreateTest(CreateTest request)
    {
        var newTest = new Test
        {
            Title = request.Title,
        };

        if (!await component.Insert(newTest))
            throw new Exception("Ошибка создания теста.");

        List<int> failedTaskIds = new List<int>();

        foreach (var taskId in request.TaskIds)
        {
            var result = await component.Insert(new TestTask
            {
                TestId = newTest.Id,
                TaskId = taskId
            });

            if (!result)
            {
                failedTaskIds.Add(taskId);
            }
        }

        if (failedTaskIds.Count > 0)
        {
            return $"Не удалось добавить задачи с ID: {string.Join(", ", failedTaskIds)}";
        }

        return $"Тест успешно создан. ID: {newTest.Id}";
    }

    public async Task<bool> SaveFileToRepo(IFormFile file)
    {
        string fileName = Path.GetFileName(file.FileName);
        
        if (component.Tasks.Any(t => t.FilePath == fileName))
            return false;
        
        var path = Path.Combine(env.ContentRootPath, "FileRepository", fileName);

        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        
        return true;
    }

    public async Task<byte[]?> GetFileBytes(string fileName)
    {
        var filePath = Path.Combine(env.ContentRootPath, "FileRepository", fileName);
        
        if (!File.Exists(filePath))
            return null;

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        
        return fileBytes;
    }
}