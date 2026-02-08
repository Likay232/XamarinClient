using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Enums;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Responses;
using WebApi.Infrastructure.Models.Storage;
using Task = System.Threading.Tasks.Task;

namespace WebApi.Services;

public class ClientService(DataComponent component, IWebHostEnvironment env)
{
    public async Task<NewData> GetNewData()
    {
        var newData = new NewData
        {
            Users = component.Users.ToList(),
            Themes = component.Themes.ToList(),
            Progresses = component.Progresses.ToList(),
            Tasks = component.Tasks.ToList(),
            CompletedTasks = component.CompletedTasks.ToList()
        };

        return newData;
    }
    public async Task<bool> UploadData<T>(List<T> data) where T : class
    {
        if (data.Count == 0) return true;
        
        if (typeof(T) == typeof(User))
        {
            var users = data.Cast<User>().ToList();
            return await UploadUsers(users);
        }

        if (typeof(T) == typeof(Progress))
        {
            var progress = data.Cast<Progress>().ToList();
            return await UploadProgress(progress);
        }

        if (typeof(T) == typeof(Infrastructure.Models.Storage.CompletedTask))
        {
            await component.BulkInsertAsync(data);
            await AlignDifficultyLevels();
            
            return true;
        }
        
        return await component.BulkInsertAsync(data);
    }

    private async Task<bool> UploadUsers(List<User> users)
    {
        var usersDb = component.Users;

        var onUpload = new List<User>();

        foreach (var user in users)
        {
            if (usersDb.Any(u => u.Username == user.Username))
                continue;

            onUpload.Add(user);
        }

        return await component.BulkInsertAsync(onUpload);
    }

    private async Task<bool> UploadProgress(List<Progress> progresses)
    {
        var progressDb = component.Progresses.ToList();

        var onUpdate = new List<Progress>();
        var onInsert = new List<Progress>();

        foreach (var progress in progresses)
        {
            var progressFromDb =
                progressDb.FirstOrDefault(p => p.UserId == progress.UserId && p.ThemeId == progress.ThemeId);

            if (progressFromDb is not null)
            {
                var higherLevel = progressFromDb.Level > progress.Level ? progressFromDb : progress;
                var lowerLevel = progressFromDb.Level > progress.Level ? progress : progressFromDb;

                var earnedXp = GetLevelCap(lowerLevel.Level) - lowerLevel.AmountToLevelUp;

                var progressToUpdate = new Progress
                {
                    Id = progressFromDb.Id,
                    UserId = progressFromDb.UserId,
                    ThemeId = progressFromDb.ThemeId,
                    Level = higherLevel.Level,
                    AmountToLevelUp = higherLevel.AmountToLevelUp - earnedXp,
                };

                var currentLevelCap = GetLevelCap(lowerLevel.Level);
                
                if (progressToUpdate.AmountToLevelUp > currentLevelCap)
                {
                    progressToUpdate.AmountToLevelUp = currentLevelCap;
                }
                else if (progressToUpdate.AmountToLevelUp <= 0)
                {
                    progressToUpdate.Level += 1;
                    progressToUpdate.AmountToLevelUp = GetLevelCap(progressToUpdate.Level);
                }

                onUpdate.Add(progressToUpdate);
            }

            else
                onInsert.Add(progress);
        }

        if (onUpdate.Any())
            await component.BulkInsertAsync(onUpdate);
        if (onInsert.Any())
            await component.BulkInsertAsync(onInsert);

        return true;
    }

    public async Task<List<UserDto>> GetNewUsers(DateTime lastClientEntryRegistration)
    {
        return component.Users
            .Where(u => u.ModifiedAt > lastClientEntryRegistration)
            .Select(u => new UserDto()
            {
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsBlocked = u.IsBlocked,
                LastLogin = u.LastLogin,
                Password = u.Password,
            })
            .ToList();
    }

    public async Task<List<Infrastructure.Models.DTO.CompletedTask>> GetNewCompletedTasks(
        DateTime lastClientEntryCompleted)
    {
        return component.CompletedTasks
            .Where(completedTask => completedTask.CompletedAt > lastClientEntryCompleted)
            .Select(completedTask => new Infrastructure.Models.DTO.CompletedTask()
            {
                UserId = completedTask.UserId,
                TaskId = completedTask.TaskId,
                CompletedAt = completedTask.CompletedAt,
                IsCorrect = completedTask.IsCorrect
            })
            .ToList();
    }

    public async Task<bool> RegisterDevice(RegisterDevice request)
    {
        if (!await component.Users.AnyAsync(u => u.Id == request.UserId))
            throw new Exception("User not found");

        if (await component.UserDevices.AnyAsync(u =>
                u.UserId == request.UserId && u.DeviceToken == request.DeviceToken))
            return true;

        var newUserDevice = new UserDevice
        {
            UserId = request.UserId,
            DeviceToken = request.DeviceToken,
        };

        return await component.Insert(newUserDevice);
    }

    public async Task<List<ThemeDto>> GetThemes()
    {
        return await component.Themes.Select(t => new ThemeDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
        }).ToListAsync();
    }

    public async Task<List<TaskForClientDto>> GetTasksForTheme(GetTasks request)
    {
        if (!await component.Themes.AnyAsync(t => t.Id == request.ThemeId))
            throw new Exception("Тема не найдена.");

        var completed = await component.CompletedTasks
            .Where(ct => ct.UserId == request.UserId)
            .ToListAsync();

        var tasks = await component.Tasks
            .Include(t => t.Theme)
            .Where(t => t.ThemeId == request.ThemeId)
            .ToListAsync();

        return tasks.Select(t =>
        {
            var completedTask = completed.FirstOrDefault(ct => ct.TaskId == t.Id);

            return new TaskForClientDto
            {
                Id = t.Id,
                ThemeId = t.ThemeId,
                ThemeName = t.Theme.Title,
                Hint = t.Hint,
                Text = t.Text,
                CorrectAnswer = t.CorrectAnswer,
                DifficultyLevel = t.DifficultyLevel,
                FilePath = t.FilePath,
                IsCorrect = completedTask?.IsCorrect ?? null,
                AnswerVariants = JsonConvert.DeserializeObject<List<string?>>(t.AnswerVariants) ?? new List<string?>()
            };
        }).ToList();
    }

    public async Task<List<LessonDto>> GetLessonsForTheme(int themeId)
    {
        return await component.Lessons
            .Include(l => l.Theme)
            .Where(l => l.ThemeId == themeId)
            .Select(l => new LessonDto
            {
                Text = l.Text,
                Link = l.Link,
                ThemeId = l.ThemeId,
                ThemeName = l.Theme.Title
            })
            .ToListAsync();
    }

    public async Task<List<TestDto>> GetTests()
    {
        return await component.Tests
            .Select(t => new TestDto
            {
                Id = t.Id,
                Title = t.Title,
            })
            .ToListAsync();
    }

    public async Task<List<TaskForClientDto>> GetTest(int testId)
    {
        if (!await component.TestTasks.AnyAsync(t => t.TestId == testId))
            throw new Exception("Вопросы для теста с заданным Id не найден.");

        return await component.TestTasks
            .Where(t => t.TestId == testId)
            .Select(t => new TaskForClientDto()
            {
                Id = t.TaskId,
                Text = t.Task.Text,
                CorrectAnswer = "",
                DifficultyLevel = t.Task.DifficultyLevel,
                FilePath = t.Task.FilePath,
            })
            .ToListAsync();
    }
    
    public async Task<TaskForClientDto> GetTaskById(int taskId)
    {
        var taskFromDb = await component.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);

        if (taskFromDb == null)
            throw new Exception("Задание с заданным id не найдено.");

        var task = new TaskForClientDto
        {
            Id = taskId,
            Text = taskFromDb.Text,
            DifficultyLevel = taskFromDb.DifficultyLevel,
            FilePath = taskFromDb.FilePath,
            IsCorrect = false
        };

        return task;
    }

    public async Task<TaskDto> GetRandomTask()
    {
        var tasks = await component.Tasks.ToListAsync();

        if (tasks.Count == 0)
            throw new Exception("Задания не найдены в базе данных.");

        var random = new Random();

        var randomTask = tasks[random.Next(tasks.Count)];

        return new TaskDto
        {
            Id = randomTask.Id,
            ThemeId = randomTask.ThemeId,
            Text = randomTask.Text,
            DifficultyLevel = randomTask.DifficultyLevel,
            FilePath = randomTask.FilePath,
        };
    }
    
    public async Task<bool> ChangePassword(ChangePasswordClient request)
    {
        var user = await component.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user == null) return false;

        if (user.Password != request.OldPassword) return false;

        user.Password = request.NewPassword;

        return await component.Update(user);
    }
    
    public async Task<byte[]?> GetFileBytes(string fileName)
    {
        var filePath = Path.Combine(env.ContentRootPath, "FileRepository", fileName);

        if (!File.Exists(filePath))
            return null;

        var fileBytes = await File.ReadAllBytesAsync(filePath);

        return fileBytes;
    }
    
    public async Task<List<ThemesStatistic>> GetStatisticForThemes(int userId)
    {
        var progresses = await component.Progresses.ToListAsync();

        var tasks = await component.Tasks
            .Include(t => t.Theme)
            .ToListAsync();

        var completedTasks = await component.CompletedTasks
            .Where(x => x.UserId == userId)
            .GroupBy(x => x.TaskId)
            .Select(g => g
                .OrderByDescending(x => x.CompletedAt)
                .First())
            .ToDictionaryAsync(x => x.TaskId, x => x.IsCorrect == true);

        var statistic = tasks
            .GroupBy(t => new { t.ThemeId, t.Theme.Title })
            .ToDictionary(g => g.Key, g =>
            {
                var total = g.Count();
                var solved = g.Count(t => completedTasks.ContainsKey(t.Id));
                var solvedCorrect = g.Count(t => completedTasks.TryGetValue(t.Id, out var cor) && cor);

                var solvedPercent = total == 0 ? 0.0 : (double)solved / total * 100;
                var correctPercent = solved == 0 ? 0.0 : (double)solvedCorrect / solved * 100;

                return new ThemesStatistic
                {
                    SolvedPercent = Math.Round(solvedPercent, 0),
                    SolvedCorrectPercent = Math.Round(correctPercent, 0),
                    ThemeId = g.Key.ThemeId,
                    ThemeName = g.Key.Title,
                    Level = progresses.FirstOrDefault(p => p.ThemeId == g.Key.ThemeId)?.Level ?? 1
                };
            });

        return statistic.Values.ToList();
    }

    public async Task<ProfileInfo> GetProfileInfo(int userId)
    {
        var user = await component.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) throw new Exception("Информация о пользователе не найдена");

        var themeStat = await GetStatisticForThemes(user.Id);

        return new ProfileInfo
        {
            LastName = user.LastName,
            FirstName = user.FirstName,
            Username = user.Username,
            ThemesStatistics = themeStat,
        };
    }

    public async Task<TestForClientDto> GenerateTest(TestTypes testType, int themeId, int userId)
    {
        switch (testType)
        {
            case TestTypes.Themes:
                return await GenerateTestForTheme(themeId);
            case TestTypes.Marathon:
                return await GenerateTestForMarathon();
            case TestTypes.Exam:
                return await GenerateTestForExam();
            case TestTypes.ChallengingQuestions:
                return await GenerateTestForChallengingQuestions(userId);
            default: throw new ArgumentOutOfRangeException(nameof(testType));
        }
    }

    private async Task<TestForClientDto> GenerateTestForTheme(int themeId)
    {
        var tasks = await component.Tasks
            .Where(t => t.ThemeId == themeId)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                ThemeId = t.ThemeId,
                Text = t.Text,
                CorrectAnswer = t.CorrectAnswer,
                DifficultyLevel = t.DifficultyLevel,
                FilePath = t.FilePath,
                AnswerVariants = JsonConvert.DeserializeObject<List<string?>>(t.AnswerVariants) ?? new List<string?>(),
                Hint = t.Hint
            })
            .ToListAsync();

        return new TestForClientDto()
        {
            Tasks = tasks
        };
    }

    private async Task<TestForClientDto> GenerateTestForMarathon()
    {
        var tasks = await component.Tasks
            .OrderBy(t => EF.Functions.Random())
            .Take(800)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                ThemeId = t.ThemeId,
                Text = t.Text,
                CorrectAnswer = t.CorrectAnswer,
                DifficultyLevel = t.DifficultyLevel,
                FilePath = t.FilePath,
                AnswerVariants = JsonConvert.DeserializeObject<List<string?>>(t.AnswerVariants) ?? new List<string?>(),
                Hint = t.Hint
            })
            .ToListAsync();

        return new TestForClientDto()
        {
            Tasks = tasks
        };
    }

    private async Task<TestForClientDto> GenerateTestForExam()
    {
        var themeIds = await component.Themes
            .OrderBy(t => EF.Functions.Random())
            .Select(t => t.Id)
            .Take(4)
            .ToListAsync();

        var test = new TestForClientDto();

        foreach (var themeId in themeIds)
        {
            var tasks = await component.Tasks
                .Where(t => t.ThemeId == themeId)
                .OrderBy(t => EF.Functions.Random())
                .Take(10)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    ThemeId = t.ThemeId,
                    Text = t.Text,
                    CorrectAnswer = t.CorrectAnswer,
                    DifficultyLevel = t.DifficultyLevel,
                    FilePath = t.FilePath,
                    AnswerVariants =
                        JsonConvert.DeserializeObject<List<string?>>(t.AnswerVariants)
                        ?? new List<string?>(),
                    Hint = t.Hint
                })
                .ToListAsync();

            var tasksForTest = tasks.Take(5).ToList();
            var additionalQuestionsForTheme = tasks.TakeLast(5).ToList();

            test.Tasks.AddRange(tasksForTest);
            test.AdditionalQuestions[themeId] = additionalQuestionsForTheme;
        }

        return test;
    }

    private async Task<TestForClientDto> GenerateTestForChallengingQuestions(int userId)
    {
        var userLevels = component.Progresses
            .Where(p => p.UserId == userId)
            .ToDictionary(p => p.ThemeId, p => p.Level);

        var grouped = await component.CompletedTasks
            .Where(ct => ct.UserId == userId && ct.IsCorrect == false)
            .GroupBy(ct => new
            {
                ct.TaskId,
                ct.Task.ThemeId,
                ct.Task.DifficultyLevel
            })
            .Select(g => new
            {
                g.Key.TaskId,
                g.Key.ThemeId,
                Difficulty = g.Key.DifficultyLevel,
                WrongCount = g.Count()
            })
            .ToListAsync();

        var mostChallengingQuestions = grouped
            .Select(g =>
            {
                var userLevel = userLevels.TryGetValue(g.ThemeId, out var lvl)
                    ? lvl
                    : 1;

                var distance = Math.Abs(g.Difficulty - userLevel);
                var isAbove = g.Difficulty > userLevel ? 1 : 0;

                return new
                {
                    g.TaskId,
                    g.WrongCount,
                    Distance = distance,
                    IsAbove = isAbove
                };
            })
            .OrderBy(g => g.Distance)
            .ThenBy(g => g.IsAbove)
            .ThenByDescending(g => g.WrongCount)
            .Take(20)
            .Select(g => g.TaskId)
            .ToList();

        var tasks = new List<TaskDto>();
        foreach (var taskId in mostChallengingQuestions)
        {
            var task = component.Tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null) continue;

            tasks.Add(new TaskDto
            {
                Id = task.Id,
                ThemeId = task.ThemeId,
                Text = task.Text,
                CorrectAnswer = task.CorrectAnswer,
                DifficultyLevel = task.DifficultyLevel,
                FilePath = task.FilePath,
                AnswerVariants = JsonConvert.DeserializeObject<List<string?>>(task.AnswerVariants) ??
                                 new List<string?>(),
                Hint = task.Hint
            });
        }

        return new TestForClientDto()
        {
            Tasks = tasks
        };
    }

    public async Task SaveAnswers(SaveAnswers request)
    {
        foreach (var answer in request.UserAnswers)
        {
            var task = component.Tasks.FirstOrDefault(t => t.Id == answer.TaskId);

            if (task is null)
                throw new Exception($"Не найдено задание с id {answer.TaskId}");

            if (!component.Users.Any(u => u.Id == request.UserId))
                throw new Exception($"Не найден пользователь с id {request.UserId}");

            var completedTask = new Infrastructure.Models.Storage.CompletedTask()
            {
                UserId = request.UserId,
                TaskId = answer.TaskId,
                IsCorrect = answer.IsCorrect,
                CompletedAt = DateTime.UtcNow
            };
            
            await ChangeUserProgress(task, request.UserId, answer.IsCorrect);

            await component.Insert(completedTask);
        }

        await AlignDifficultyLevels();
    }

    public async Task SaveAnswer(int userId, int taskId, bool isCorrect)
    {
        var task = component.Tasks.FirstOrDefault(t => t.Id == taskId);

        if (task is null)
            throw new Exception($"Не найдено задание с id {taskId}");

        if (!component.Users.Any(u => u.Id == userId))
            throw new Exception($"Не найден пользователь с id {userId}");

        var completedTask = new Infrastructure.Models.Storage.CompletedTask()
        {
            UserId = userId,
            TaskId = taskId,
            IsCorrect = isCorrect,
            CompletedAt = DateTime.UtcNow
        };

        await ChangeUserProgress(task, userId, isCorrect);
        await AlignDifficultyLevels();

        await component.Insert(completedTask);
    }

    private async Task<List<WebApi.Infrastructure.Models.Storage.Task>> AlignDifficultyLevels()
    {
        var tasks = component.Tasks.ToList();

        var completedTasksAmount = component.CompletedTasks
            .GroupBy(ct => ct.TaskId)
            .ToDictionary(g => g.Key, g => g.Count());

        var correctCounts = component.CompletedTasks
            .Where(ct => ct.IsCorrect == true)
            .GroupBy(ct => ct.TaskId)
            .ToDictionary(g => g.Key, g => g.Count());

        var tasksToUpdate = new List<WebApi.Infrastructure.Models.Storage.Task>();

        foreach (var task in tasks)
        {
            if (!completedTasksAmount.TryGetValue(task.Id, out var totalCompleted))
                continue;

            var correctCount = correctCounts.TryGetValue(task.Id, out var count) ? count : 0;

            var percentage = totalCompleted > 0 ? (double)correctCount / totalCompleted * 100 : 0;

            int correctLevelForPercentage;
            switch (percentage)
            {
                case >= 0 and <= 20:
                    correctLevelForPercentage = 1;
                    break;
                case > 20 and <= 40:
                    correctLevelForPercentage = 2;
                    break;
                case > 40 and <= 60:
                    correctLevelForPercentage = 3;
                    break;
                case > 60 and <= 80:
                    correctLevelForPercentage = 4;
                    break;
                case > 80 and <= 100:
                    correctLevelForPercentage = 5;
                    break;
                default:
                    correctLevelForPercentage = 1;
                    break;
            }

            if (task.DifficultyLevel == correctLevelForPercentage) continue;

            task.DifficultyLevel = correctLevelForPercentage;
            task.ModifiedAt = DateTime.UtcNow;
            tasksToUpdate.Add(task);
        }

        await component.BulkUpdateAsync(tasksToUpdate);
        
        return tasksToUpdate;
    }

    private double GetExperience(bool isCorrect, int difficultyLevel, int currentLevel)
    {
        return !isCorrect ? 10 * (double)difficultyLevel / currentLevel : 10 * (double)currentLevel / difficultyLevel;
    }

    private async Task ChangeUserProgress(WebApi.Infrastructure.Models.Storage.Task task, int userId, bool isCorrect)
    {
        var currentProgress =
            component.Progresses.FirstOrDefault(p => p.UserId == userId && p.ThemeId == task.ThemeId);

        bool toUpdate = true;

        if (currentProgress?.Level == 5) return;

        if (currentProgress is null)
        {
            toUpdate = false;

            currentProgress = new Progress
            {
                UserId = userId,
                ThemeId = task.ThemeId,
                Level = 1,
                AmountToLevelUp = 100
            };
        }

        var experience = GetExperience(isCorrect, task.DifficultyLevel, currentProgress.Level);

        currentProgress.ModifiedAt = DateTime.UtcNow;
        currentProgress.AmountToLevelUp -= experience;
        var currentLevelCap = GetLevelCap(currentProgress.Level);

        if (currentProgress.AmountToLevelUp > currentLevelCap)
            currentProgress.AmountToLevelUp = currentLevelCap;

        if (currentProgress.AmountToLevelUp <= 0)
        {
            currentProgress.Level += 1;
            currentProgress.AmountToLevelUp = GetLevelCap(currentProgress.Level);
        }

        if (toUpdate) await component.Update(currentProgress);
        else await component.Insert(currentProgress);
    }

    private double GetLevelCap(int level) => 100 * Math.Pow(2, level);

    public async Task<int> GetTaskAmount()
    {
        return await component.Tasks.CountAsync();
    }
}