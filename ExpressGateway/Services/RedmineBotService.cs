using ExpressGateway.Services.Redmine.Models;

namespace ExpressGateway.Services.Redmine;

public class RedmineBotService : IRedmineBotService
{
    private readonly IRedmineService _redmineService;
    private readonly ILogger<RedmineBotService> _logger;
    private readonly Dictionary<string, Func<string[], Task<string>>> _commands;
    private readonly Dictionary<string, string> _userStates = new();

    public RedmineBotService(
        IRedmineService redmineService,
        ILogger<RedmineBotService> logger)
    {
        _redmineService = redmineService;
        _logger = logger;

        _commands = new Dictionary<string, Func<string[], Task<string>>>
        {
            ["/start"] = HandleStartAsync,
            ["/help"] = HandleHelpAsync,
            ["/status"] = HandleStatusAsync,
            ["/create"] = HandleCreateAsync,
            ["/issues"] = HandleIssuesAsync,
            ["/stop"] = HandleStopAsync
        };
    }

    public async Task<string> ProcessMessageAsync(string message, string senderName, string? chatId = null)
    {
        try
        {
            _logger.LogInformation("Processing message from {Sender}: {Message}", senderName, message);

            if (string.IsNullOrWhiteSpace(message))
                return "Пожалуйста, напишите сообщение.";

            if (message.StartsWith("/"))
            {
                var parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var command = parts[0].ToLower();
                var args = parts.Skip(1).ToArray();

                if (_commands.TryGetValue(command, out var handler))
                    return await handler(args);

                return await HandleUnknownCommandAsync(args);
            }

            if (_userStates.TryGetValue(senderName, out var state))
            {
                return await HandleStateAsync(state, message, senderName);
            }

            return await AnalyzeAndRespondAsync(message, senderName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            return $"Произошла ошибка: {ex.Message}";
        }
    }

    private async Task<string> AnalyzeAndRespondAsync(string message, string senderName)
    {
        if (message.Contains("задача", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("тикет", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("создать", StringComparison.OrdinalIgnoreCase))
        {
            return await StartIssueCreationAsync(senderName);
        }

        if (message.Contains("статус", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("проверить", StringComparison.OrdinalIgnoreCase))
        {
            return await GetUserIssuesAsync(null, null);
        }

        if (message.Contains("помощь", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("help", StringComparison.OrdinalIgnoreCase))
        {
            return await GetHelpAsync();
        }

        if (message.Contains("привет", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("здравствуйте", StringComparison.OrdinalIgnoreCase))
        {
            return await GetGreetingAsync(senderName);
        }

        if (message.Contains("список", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("все задачи", StringComparison.OrdinalIgnoreCase))
        {
            return await GetUserIssuesAsync(null, null);
        }

        return "Я не понял ваш запрос. Напишите /help для списка доступных команд.";
    }

    private async Task<string> StartIssueCreationAsync(string userName)
    {
        _userStates[userName] = "creating_issue_subject";
        return "Для создания заявки напишите тему (краткое описание проблемы):\n" +
               "Например: 'Не работает принтер в отделе разработки'";
    }

    private async Task<string> HandleStateAsync(string state, string input, string userName)
    {
        switch (state)
        {
            case "creating_issue_subject":
                _userStates[userName] = "creating_issue_description";
                return $"Тема: {input}\n\nТеперь напишите описание заявки (подробности проблемы):";

            case "creating_issue_description":
                _userStates.Remove(userName);
                return await CreateIssueAsync(input, "Описание заявки от пользователя", null);
        }

        _userStates.Remove(userName);
        return "Состояние сброшено. Напишите /help для списка команд.";
    }

    public async Task<string> GetIssueStatusAsync(int issueId)
    {
        try
        {
            var issue = await _redmineService.GetIssueAsync(issueId);
            if (issue == null)
                return $"Заявка #{issueId} не найдена.";

            return $"Заявка #{issue.Id}\n" +
                   $"Тема: {issue.Subject}\n" +
                   $"Статус: {issue.Status?.Name ?? "Не указан"}\n" +
                   $"Приоритет: {issue.Priority?.Name ?? "Не указан"}\n" +
                   $"Проект: {issue.Project?.Name ?? "Не указан"}\n" +
                   $"Создана: {issue.CreatedOn:dd.MM.yyyy HH:mm}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get issue status");
            return $"Ошибка получения статуса заявки: {ex.Message}";
        }
    }

    public async Task<string> CreateIssueAsync(string subject, string description, string? categoryId = null)
    {
        try
        {
            var issue = new RedmineIssue
            {
                Subject = subject,
                Description = description,
                Project = await _redmineService.GetProjectAsync("req")
            };

            if (issue.Project == null)
                return "Не удалось найти проект для создания заявки.";

            var created = await _redmineService.CreateIssueAsync(issue);
            return $"Заявка успешно создана!\n" +
                   $"Номер: #{created.Id}\n" +
                   $"Тема: {created.Subject}\n" +
                   $"Для проверки статуса используйте: /status {created.Id}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create issue");
            return $"Ошибка создания заявки: {ex.Message}";
        }
    }

    public async Task<string> GetUserIssuesAsync(string? city = null, string? department = null)
    {
        try
        {
            var issues = await _redmineService.GetIssuesAsync();
            
            if (!issues.Any())
                return "У вас нет активных заявок.";

            var result = $"Ваши заявки ({issues.Count}):\n" +
                        "─────────────────────\n";

            foreach (var issue in issues.Take(10))
            {
                result += $"#{issue.Id} | {issue.Status?.Name ?? "Не указан"} | {issue.Subject}\n";
            }

            if (issues.Count > 10)
                result += $"... и еще {issues.Count - 10} заявок";

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user issues");
            return $"Ошибка получения списка заявок: {ex.Message}";
        }
    }

    public async Task<string> GetHelpAsync()
    {
        return "Доступные команды:\n" +
               "/start - начать работу\n" +
               "/help - показать эту справку\n" +
               "/status [номер] - статус заявки\n" +
               "/create - создать новую заявку\n" +
               "/issues - список ваших заявок\n" +
               "/stop - завершить сессию\n\n" +
               "Вы также можете использовать обычные сообщения:\n" +
               "- 'Создать заявку' - начать создание\n" +
               "- 'Статус заявки' - список заявок\n" +
               "- 'Помощь' - показать справку";
    }

    public async Task<string> GetGreetingAsync(string? userName = null)
    {
        var greeting = GetTimeBasedGreeting();
        var welcome = string.IsNullOrEmpty(userName) 
            ? "Добро пожаловать в Redmine бот!" 
            : $"Добро пожаловать, {userName}!";

        return $"{greeting}, {welcome}\n\n{await GetHelpAsync()}";
    }

    private string GetTimeBasedGreeting()
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
    private async Task<string> HandleStartAsync(string[] args)
    {
        return await GetGreetingAsync();
    }

    private async Task<string> HandleHelpAsync(string[] args)
    {
        return await GetHelpAsync();
    }

    private async Task<string> HandleStatusAsync(string[] args)
    {
        if (args.Length == 0 || !int.TryParse(args[0], out var issueId))
            return "Пожалуйста, укажите номер заявки: /status 12345";

        return await GetIssueStatusAsync(issueId);
    }

    private async Task<string> HandleCreateAsync(string[] args)
    {
        if (args.Length < 2)
            return "Используйте: /create [тема] [описание]\n" +
                   "Например: /create Не работает принтер Нет питания в розетке";

        var subject = args[0];
        var description = string.Join(" ", args.Skip(1));
        
        return await CreateIssueAsync(subject, description);
    }

    private async Task<string> HandleIssuesAsync(string[] args)
    {
        return await GetUserIssuesAsync();
    }

    private async Task<string> HandleStopAsync(string[] args)
    {
        return "Работа бота завершена. Для начала работы используйте /start";
    }

    private async Task<string> HandleUnknownCommandAsync(string[] args)
    {
        return "Неизвестная команда. Напишите /help для списка доступных команд.";
    }
}