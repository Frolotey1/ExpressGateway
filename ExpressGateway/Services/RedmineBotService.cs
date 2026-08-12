using ExpressGateway.Services.Redmine.Models;

namespace ExpressGateway.Services.Redmine;

public class RedmineBotService : IRedmineBotService
{
    private readonly IRedmineService _redmineService;
    private readonly ILogger<RedmineBotService> _logger;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, Func<string[], Task<string>>> _commands;
    private readonly Dictionary<string, string> _userStates = new();
    private readonly Dictionary<string, DateTime> _blockedUsers = new();
    private readonly Dictionary<string, int> _errorCount = new();
    private readonly Dictionary<string, string> _sessions = new();
    private readonly Dictionary<string, object> _sessionData = new();
    private readonly string _projectIdentifier;
    private readonly bool _isDevelopment;
    private const int BlockTimeSeconds = 600;
    private const int InactivityTimerSeconds = 120;

    public RedmineBotService(
        IRedmineService redmineService,
        ILogger<RedmineBotService> logger,
        IConfiguration configuration)
    {
        _redmineService = redmineService;
        _logger = logger;
        _configuration = configuration;

        _projectIdentifier = _configuration["RedmineSettings:ProjectIdentifier"] ?? "req";
        _isDevelopment = configuration.GetValue<bool>("IsDevelopment", false);

        _logger.LogInformation($"RedmineBotService initialized. Project: {_projectIdentifier}, IsDevelopment: {_isDevelopment}");

        _commands = new Dictionary<string, Func<string[], Task<string>>>
        {
            ["/start"] = HandleStartAsync,
            ["/stop"] = HandleStopAsync,
            ["/help"] = HandleHelpAsync,
            ["/status"] = HandleStatusAsync,
            ["/status_with_token"] = HandleStatusWithTokenAsync,
            ["/issues"] = HandleIssuesAsync,
            ["/create_issue"] = HandleCreateIssueAsync,
            ["/custom_fields"] = HandleCustomFieldsAsync,
            ["/issue_direction"] = HandleIssueDirectionAsync,
            ["/add_direction"] = HandleAddDirectionAsync,
            ["/choise"] = HandleChoiseAsync,
            ["/add_attachment"] = HandleAddAttachmentAsync,
            ["/get_custom_field"] = HandleGetCustomFieldAsync,
            ["/create_issue_custom_fields"] = HandleCreateIssueCustomFieldsAsync
        };

        if (_isDevelopment)
        {
            _commands["/create_test_issue"] = HandleCreateTestIssueAsync;
            _logger.LogInformation("Test command '/create_test_issue' enabled (Development mode)");
        }
    }

    public async Task<string> ProcessMessageAsync(string message, string senderName)
    {
        try
        {
            _logger.LogInformation("Processing message from {Sender}: {Message}", senderName, message);

            if (IsUserBlocked(senderName))
                return $"Вы заблокированы на {BlockTimeSeconds / 60} минут";

            if (string.IsNullOrWhiteSpace(message))
                return "Пожалуйста, напишите сообщение.";

            StartInactivityTimer(senderName);

            if (message.StartsWith("/"))
            {
                var parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var command = parts[0].ToLower();
                var args = parts.Skip(1).ToArray();

                if (_commands.TryGetValue(command, out var handler))
                    return await handler(args);

                return await HandleUnknownCommandAsync(args, senderName);
            }

            if (_userStates.TryGetValue(senderName, out var state))
                return await HandleStateAsync(state, message, senderName);

            return await HandleMessageAsync(message, senderName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            return $"Произошла ошибка: {ex.Message}";
        }
    }

    private string GetTimeOfDay()
    {
        var hour = DateTime.Now.Hour;
        return hour switch
        {
            >= 0 and < 6 => "Доброй ночи",
            >= 6 and < 12 => "Доброе утро",
            >= 12 and < 18 => "Добрый день",
            >= 18 and < 24 => "Добрый вечер",
            _ => "Здравствуйте"
        };
    }

    private string TimeToFrase() => GetTimeOfDay();
    public async Task<string> CreateIssueAsync(string subject, string description, string? categoryId = null)
    {
        try
        {
            var project = await _redmineService.GetProjectAsync(_projectIdentifier);
            if (project == null)
                return $"Не удалось найти проект '{_projectIdentifier}' для создания заявки.";

            var issue = new RedmineIssue
            {
                Subject = subject,
                Description = description,
                Project = project,
                Tracker = new RedmineTracker { Id = 1 },
                Status = new RedmineStatus { Id = 1 },
                Priority = new RedminePriority { Id = 2 }
            };

            if (!string.IsNullOrEmpty(categoryId) && int.TryParse(categoryId, out var catId))
            {
                issue.CategoryId = catId;
            }

            var created = await _redmineService.CreateIssueAsync(issue);
            return $"Заявка успешно создана! Номер: {created.Id}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create issue");
            return $"Ошибка создания заявки: {ex.Message}";
        }
    }
    private async Task<string> CreateTestIssueAsync(string subject, string description)
    {
        try
        {
            _logger.LogInformation($"Creating test issue: {subject} - {description}");
            
            var fakeId = new Random().Next(1000, 9999);
            var fakeDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            
            return $"ТЕСТОВАЯ ЗАЯВКА\n" +
                   $"─────────────────────\n" +
                   $"Номер: {fakeId}\n" +
                   $"Тема: {subject}\n" +
                   $"Описание: {description}\n" +
                   $"Создана: {fakeDate}\n" +
                   $"Статус: Новая\n" +
                   $"─────────────────────\n" +
                   $"Тестовый режим. Заявка НЕ создана в Redmine.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create test issue");
            return $"Ошибка создания тестовой заявки: {ex.Message}";
        }
    }

    private async Task<string> HandleCreateTestIssueAsync(string[] args)
    {
        if (args.Length < 2)
            return "Используйте: /create_test_issue [тема] [описание]";

        var subject = args[0];
        var description = string.Join(" ", args.Skip(1));
        
        return await CreateTestIssueAsync(subject, description);
    }

    private void StartInactivityTimer(string senderName)
    {
        _sessions[senderName] = "active";
        _ = Task.Run(async () =>
        {
            await Task.Delay(InactivityTimerSeconds * 1000);
            if (_sessions.TryGetValue(senderName, out var state) && state == "active")
            {
                _sessions.Remove(senderName);
                _logger.LogInformation("Inactivity timer triggered for {Sender}", senderName);
            }
        });
    }

    private bool IsUserBlocked(string senderName)
    {
        if (_blockedUsers.TryGetValue(senderName, out var blockTime))
        {
            if ((DateTime.UtcNow - blockTime).TotalSeconds < BlockTimeSeconds)
                return true;
            
            _blockedUsers.Remove(senderName);
            _errorCount.Remove(senderName);
        }
        return false;
    }

    private async Task<string> BlockUserAsync(string senderName)
    {
        _blockedUsers[senderName] = DateTime.UtcNow;
        _ = Task.Run(async () =>
        {
            await Task.Delay(BlockTimeSeconds * 1000);
            _blockedUsers.Remove(senderName);
            _errorCount.Remove(senderName);
        });
        return $"Вы заблокированы на {BlockTimeSeconds / 60} минут";
    }

    private void IncrementErrorCount(string senderName)
    {
        if (!_errorCount.ContainsKey(senderName))
            _errorCount[senderName] = 0;
        _errorCount[senderName]++;
        if (_errorCount[senderName] >= 3)
            _ = BlockUserAsync(senderName);
    }

    private async Task<string> HandleStartAsync(string[] args)
    {
        var senderName = args.Length > 0 ? args[0] : "default";
        _errorCount.Remove(senderName);
        _blockedUsers.Remove(senderName);
        _userStates.Remove(senderName);
        _sessions.Remove(senderName);

        var greeting = TimeToFrase();
        
        var commands = "Доступные команды:\n" +
                       "/start - начать работу\n" +
                       "/stop - завершить сессию\n" +
                       "/help - показать справку\n" +
                       "/status [номер] - статус заявки\n" +
                       "/status_with_token - статус с токеном\n" +
                       "/issues - список ваших заявок\n" +
                       "/create_issue - создать новую заявку\n" +
                       "/custom_fields - кастомные поля\n" +
                       "/issue_direction - направление заявки\n" +
                       "/add_direction - добавить направление\n" +
                       "/choise - выбор\n" +
                       "/add_attachment - добавить вложение\n" +
                       "/get_custom_field - получить кастомное поле\n" +
                       "/create_issue_custom_fields - создать кастомное поле";

        if (_isDevelopment)
        {
            commands += "\n/create_test_issue - создать ТЕСТОВУЮ заявку (только для разработки)";
        }

        return $"{greeting}! Приветствую!\n\n{commands}";
    }

    private async Task<string> HandleStopAsync(string[] args)
    {
        var senderName = args.Length > 0 ? args[0] : "default";
        _userStates.Remove(senderName);
        _errorCount.Remove(senderName);
        _blockedUsers.Remove(senderName);
        _sessions.Remove(senderName);
        return "До свидания!";
    }

    private async Task<string> HandleHelpAsync(string[] args)
    {
        var commands = "/start - начать работу\n" +
                       "/stop - завершить сессию\n" +
                       "/help - показать эту справку\n" +
                       "/status [номер] - статус заявки\n" +
                       "/status_with_token - статус с токеном\n" +
                       "/issues - список ваших заявок\n" +
                       "/create_issue - создать новую заявку\n" +
                       "/custom_fields - кастомные поля\n" +
                       "/issue_direction - направление заявки\n" +
                       "/add_direction - добавить направление\n" +
                       "/choise - выбор\n" +
                       "/add_attachment - добавить вложение\n" +
                       "/get_custom_field - получить кастомное поле\n" +
                       "/create_issue_custom_fields - создать кастомное поле";

        if (_isDevelopment)
        {
            commands += "\n/create_test_issue - создать ТЕСТОВУЮ заявку (только для разработки)";
        }

        return "Доступные команды:\n" + commands;
    }

    private async Task<string> HandleStatusAsync(string[] args)
    {
        if (args.Length == 0)
            return "Введите ФИО для поиска заявки:";

        if (int.TryParse(args[0], out var issueId))
            return await GetIssueStatusAsync(issueId);

        return "Пожалуйста, укажите номер заявки: /status 12345";
    }

    private async Task<string> HandleStatusWithTokenAsync(string[] args)
    {
        var senderName = args.Length > 0 ? args[0] : "default";
        if (_userStates.TryGetValue(senderName + "_token", out var token) && token != "no_data")
            return "Введите номер заявки в формате 98765432121 (только цифры)";
        return "Для начала выполните /status";
    }

    private async Task<string> HandleIssuesAsync(string[] args)
    {
        try
        {
            _logger.LogInformation("Getting issues from Redmine...");
            
            var issues = await _redmineService.GetIssuesAsync();
            
            _logger.LogInformation($"Found {issues?.Count ?? 0} issues");
            
            if (issues == null || !issues.Any())
                return "У вас нет активных заявок.";

            var result = $"Ваши заявки ({issues.Count}):\n─────────────────────\n";
            foreach (var issue in issues.Take(10))
                result += $"#{issue.Id} | {issue.Status?.Name ?? "Не указан"} | {issue.Subject}\n";
            if (issues.Count > 10)
                result += $"... и еще {issues.Count - 10} заявок";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting issues");
            return $"Ошибка получения заявок: {ex.Message}";
        }
    }

    private async Task<string> HandleCreateIssueAsync(string[] args)
    {
        if (args.Length < 2)
            return "Используйте: /create_issue [тема] [описание]";

        var subject = args[0];
        var description = string.Join(" ", args.Skip(1));
        
        return await CreateIssueAsync(subject, description);
    }

    private async Task<string> HandleCustomFieldsAsync(string[] args)
    {
        try
        {
            var fields = await _redmineService.GetCustomFieldsAsync(_projectIdentifier);
            if (!fields.Any())
                return "Кастомные поля не найдены.";
            var result = "Доступные кастомные поля:\n─────────────────────\n";
            foreach (var field in fields)
                result += $"ID: {field.Id} | {field.Name}\n";
            return result;
        }
        catch (Exception ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }

    private async Task<string> HandleIssueDirectionAsync(string[] args)
    {
        try
        {
            var categories = await _redmineService.GetIssueCategoriesAsync(_projectIdentifier);
            if (!categories.Any())
                return "Категории не найдены.";
            var result = "Выберите направление:\n─────────────────────\n";
            foreach (var category in categories)
                result += $"ID: {category.Id} | {category.Name}\n";
            return result;
        }
        catch (Exception ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }

    private async Task<string> HandleAddDirectionAsync(string[] args)
    {
        if (args.Length == 0)
            return "Используйте: /add_direction [направление]";
        var senderName = args.Length > 0 ? args[0] : "default";
        _sessionData[senderName + "_direction"] = args[0];
        return $"Направление {args[0]} выбрано. Отправить заявку?";
    }

    private async Task<string> HandleChoiseAsync(string[] args)
    {
        return "Добавить дополнительные сведения?";
    }

    private async Task<string> HandleAddAttachmentAsync(string[] args)
    {
        return "Загрузите файл для прикрепления к заявке.";
    }

    private async Task<string> HandleGetCustomFieldAsync(string[] args)
    {
        if (args.Length == 0)
            return "Используйте: /get_custom_field [id]";
        if (!int.TryParse(args[0], out var id))
            return "Введите числовой ID";
        try
        {
            var fields = await _redmineService.GetCustomFieldsAsync(_projectIdentifier);
            var field = fields.FirstOrDefault(f => f.Id == id);
            return field != null 
                ? $"Поле: {field.Name}\nID: {field.Id}\nФормат: {field.FieldFormat}\nОбязательное: {field.IsRequired}"
                : "Поле не найдено";
        }
        catch (Exception ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }

    private async Task<string> HandleCreateIssueCustomFieldsAsync(string[] args)
    {
        if (args.Length < 2)
            return "Используйте: /create_issue_custom_fields [id] [значение]";
        var senderName = args.Length > 0 ? args[0] : "default";
        var id = args[0];
        var value = string.Join(" ", args.Skip(1));
        return $"Кастомное поле {id} установлено в: {value}";
    }

    private async Task<string> HandleStateAsync(string state, string input, string userName)
    {
        switch (state)
        {
            case "set_last_name":
                var parts = input.Split(' ');
                if (parts.Length >= 2)
                {
                    _userStates[userName + "_firstname"] = parts[0];
                    _userStates[userName + "_lastname"] = parts[1];
                    if (parts.Length >= 3)
                        _userStates[userName + "_other_name"] = parts[2];
                    _userStates[userName] = "enter_city";
                    return "Введите ваш город:";
                }
                return "Введите ФИО (Имя Фамилия Отчество):";

            case "enter_city":
                _userStates[userName + "_city"] = input;
                _userStates.Remove(userName);
                var fio = $"{_userStates[userName + "_firstname"]} {_userStates[userName + "_lastname"]}";
                var user = await _redmineService.GetUserByFullNameAsync(
                    _userStates[userName + "_firstname"],
                    _userStates[userName + "_lastname"]
                );
                if (user != null)
                    return $"{TimeToFrase()} {fio}, для продолжения оформления заявки, новым сообщением напишите тему Вашей заявки.";
                return $"Пользователь {fio} не найден в системе!";
                
            case "create_issue_subject":
                _userStates[userName + "_new_issue_description"] = input;
                _userStates.Remove(userName);
                return await CreateIssueAsync(
                    _userStates[userName + "_new_issue_subject"],
                    input
                );
        }
        _userStates.Remove(userName);
        return "Состояние сброшено. Напишите /help для списка команд.";
    }

    private async Task<string> HandleMessageAsync(string message, string senderName)
    {
        var lower = message.ToLower();

        switch (lower)
        {
            case "список доступных комманд":
                return await HandleHelpAsync(new string[] { });
            case "создать заявку":
                return await HandleCreateIssueAsync(new string[] { });
            case "узнать статус заявки":
                return "Введите номер заявки: /status 12345";
            default:
                IncrementErrorCount(senderName);
                if (IsUserBlocked(senderName))
                    return $"Вы заблокированы на {BlockTimeSeconds / 60} минут";
                return "Возможно вы имели ввиду?\n/start - начать работу\n/help - показать справку\n/status [номер] - статус заявки\n/issues - список ваших заявок\n/create_issue - создать новую заявку\n/stop - завершить сессию";
        }
    }

    private async Task<string> HandleUnknownCommandAsync(string[] args, string senderName)
    {
        IncrementErrorCount(senderName);
        if (IsUserBlocked(senderName))
            return $"Вы заблокированы на {BlockTimeSeconds / 60} минут";
        return "Неизвестная команда. Напишите /help для списка доступных команд.";
    }

    public async Task<string> GetIssueStatusAsync(int issueId)
    {
        try
        {
            var issue = await _redmineService.GetIssueAsync(issueId);
            if (issue == null) return $"Заявка #{issueId} не найдена.";
            return $"Заявка #{issue.Id}\nПроект: {issue.Project?.Name ?? "Не указан"}\nТип: {issue.Tracker?.Name ?? "Не указан"}\nСтатус: {issue.Status?.Name ?? "Не указан"}\nПриоритет: {issue.Priority?.Name ?? "Не указан"}\nАвтор: {issue.Author?.FullName ?? "Не указан"}\nТема: {issue.Subject}\nОписание: {issue.Description}\nСоздана: {issue.CreatedOn:dd.MM.yyyy HH:mm}";
        }
        catch (Exception ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }

    public async Task<string> CreateIssueAsync(string subject, string description)
    {
        try
        {
            var project = await _redmineService.GetProjectAsync(_projectIdentifier);
            if (project == null) 
                return $"Не удалось найти проект '{_projectIdentifier}' для создания заявки.";

            var issue = new RedmineIssue
            {
                Subject = subject,
                Description = description,
                Project = project,
                Tracker = new RedmineTracker { Id = 1 },
                Status = new RedmineStatus { Id = 1 },
                Priority = new RedminePriority { Id = 2 }
            };

            var created = await _redmineService.CreateIssueAsync(issue);
            return $"Заявка успешно создана! Номер: {created.Id}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create issue");
            return $"Ошибка: {ex.Message}";
        }
    }

    public async Task<string> GetUserIssuesAsync(string? city = null, string? department = null)
        => await HandleIssuesAsync(new string[] { });

    public async Task<string> GetHelpAsync() => await HandleHelpAsync(new string[] { });

    public async Task<string> GetGreetingAsync(string? userName = null)
        => $"{TimeToFrase()}! {userName ?? "Добро пожаловать"}!";

    public async Task<List<CommandInfo>> GetAvailableCommandsAsync()
    {
        var commands = new List<CommandInfo>
        {
            new CommandInfo { Name = "Help", Description = "Помощь", Body = "/help" },
            new CommandInfo { Name = "Start", Description = "Начать работу", Body = "/start" },
            new CommandInfo { Name = "Stop", Description = "Завершить сессию", Body = "/stop" },
            new CommandInfo { Name = "Status", Description = "Статус заявки", Body = "/status [номер]" },
            new CommandInfo { Name = "Issues", Description = "Список заявок", Body = "/issues" },
            new CommandInfo { Name = "Create Issue", Description = "Создать заявку", Body = "/create_issue [тема] [описание]" },
            new CommandInfo { Name = "Custom Fields", Description = "Кастомные поля", Body = "/custom_fields" },
            new CommandInfo { Name = "Issue Direction", Description = "Направление заявки", Body = "/issue_direction" },
            new CommandInfo { Name = "Add Direction", Description = "Добавить направление", Body = "/add_direction [направление]" },
            new CommandInfo { Name = "Choise", Description = "Выбор", Body = "/choise" },
            new CommandInfo { Name = "Add Attachment", Description = "Добавить вложение", Body = "/add_attachment" },
            new CommandInfo { Name = "Get Custom Field", Description = "Получить кастомное поле", Body = "/get_custom_field [id]" },
            new CommandInfo { Name = "Create Issue Custom Fields", Description = "Создать с кастомными полями", Body = "/create_issue_custom_fields [id] [значение]" },
            new CommandInfo { Name = "Status With Token", Description = "Статус с токеном", Body = "/status_with_token" }
        };

        if (_isDevelopment)
        {
            commands.Add(new CommandInfo 
            { 
                Name = "Create Test Issue (DEV)", 
                Description = "Создать ТЕСТОВУЮ заявку (только для разработки)", 
                Body = "/create_test_issue [тема] [описание]" 
            });
        }

        return commands;
    }
}