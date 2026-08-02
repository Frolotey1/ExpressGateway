using ExpressGateway.Middleware;
using ExpressGateway.Services;
using ExpressGateway.Services.Redmine;
using ExpressGateway.Services.Redmine.Models;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Express Gateway API",
        Version = "v1.0.0",
        Description = "API для интеграции с Express мессенджером",
    });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Api-Key",
        Description = "Введите ваш API ключ для доступа к API Express"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

builder.Services.AddControllers();
builder.Services.AddHttpClient<IExpressService, ExpressService>();
builder.Services.AddHttpClient<IRedmineService, RedmineService>();
builder.Services.AddScoped<IExpressService, ExpressService>();
builder.Services.AddScoped<RedmineBotService>();
builder.Services.AddScoped<IRedmineService, RedmineService>();
builder.Services.AddScoped<MessageProcessorService>();
builder.Services.AddHostedService<MessagePollingService>();
builder.Services.AddScoped<IRedmineBotService, RedmineBotService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Express Gateway API v1");
        c.RoutePrefix = "swagger";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.EnableTryItOutByDefault();
        c.DisplayRequestDuration();
        c.DefaultModelsExpandDepth(-1);
        
        c.HeadContent = @"
        <style>
            .custom-info {
                background: #1e293b;
                color: #e2e8f0;
                padding: 20px 30px;
                margin: 20px 0;
                border-radius: 8px;
                border-left: 4px solid #3b82f6;
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            }
            .custom-info h1 { color: #60a5fa; margin-top: 0; font-size: 24px; }
            .custom-info h2 { color: #93c5fd; font-size: 18px; margin-top: 20px; }
            .custom-info h3 { color: #93c5fd; font-size: 16px; margin-top: 15px; }
            .custom-info code { background: #0f172a; padding: 2px 8px; border-radius: 4px; color: #fcd34d; font-size: 14px; }
            .custom-info .highlight { background: #0f172a; padding: 10px 15px; border-radius: 4px; border-left: 3px solid #fcd34d; margin: 5px 0; }
            .custom-info .warning { background: #451a1a; border-left: 4px solid #ef4444; padding: 10px 15px; border-radius: 4px; margin: 10px 0; }
            .custom-info .success { background: #064e3b; border-left: 4px solid #10b981; padding: 10px 15px; border-radius: 4px; margin: 10px 0; }
            .custom-info .info { background: #1a365d; border-left: 4px solid #3b82f6; padding: 10px 15px; border-radius: 4px; margin: 10px 0; }
            .custom-info .badge { display: inline-block; background: #3b82f6; color: white; padding: 2px 10px; border-radius: 12px; font-size: 12px; font-weight: bold; }
            .custom-info .endpoint { color: #34d399; font-weight: bold; }
            .custom-info .method { color: #f472b6; font-weight: bold; }
            .custom-info .example { background: #0f172a; padding: 8px 12px; border-radius: 4px; margin: 5px 0; font-family: monospace; color: #a5f3fc; }
            .custom-info .param { color: #fcd34d; font-weight: bold; }
            .custom-info .required { color: #ef4444; font-weight: bold; }
        </style>
        <div class='custom-info'>
            <h1>Аутентификация</h1>
            
            <div class='warning'>
                <p><strong>ВАЖНО ДЛЯ SWAGGER:</strong></p>
                <p>1. Нажмите <strong>Authorize</strong> в правом верхнем углу</p>
                <p>2. Введите ваш API ключ: <code>your-api-key-here</code></p>
                <p>3. Нажмите <strong>Authorize</strong></p>
                <p>4. <strong>ОБНОВИТЕ СТРАНИЦУ (F5)</strong></p>
                <p>5. Затем нажмите <strong>Try it out</strong> на любом эндпоинте</p>
            </div>

            <h2>Информация о боте</h2>
            <div class='info'>
                <p><strong>Bot ID:</strong> <code>your-bot-id</code> <span class='required'>(обязательный)</span></p>
                <p><strong>SecKey:</strong> <code>your-sec-key</code> <span class='required'>(обязательный)</span></p>
                <p><strong>API Url:</strong> <code>https://x.ar-management.ru</code> <span class='required'>(обязательный)</span></p>
                <p><small>Эти параметры задаются в appsettings.json и используются для авторизации бота</small></p>
            </div>

            <h2>Доступные эндпоинты</h2>
            
            <h3>Методы для работы с ботом</h3>
            <div class='highlight'>
                <p><span class='method'>GET</span> <span class='endpoint'>/api/Messenger/health</span> - Проверка здоровья сервиса</p>
                <p><span class='method'>GET</span> <span class='endpoint'>/api/Messenger/status</span> - Получить статус чата с ботом</p>
                <p><span class='method'>POST</span> <span class='endpoint'>/api/Messenger/command</span> - Отправить команду боту</p>
            </div>

            <h3>Вспомогательные методы</h3>
            <div class='highlight'>
                <p><span class='method'>GET</span> <span class='endpoint'>/api/Messenger/ping</span> - Проверка доступности API</p>
            </div>

            <h3>ℹСистемные методы</h3>
            <div class='highlight'>
                <p><span class='method'>GET</span> <span class='endpoint'>/health</span> - Health check сервиса</p>
            </div>

            <h2>Примеры запросов к боту</h2>
            
            <h3>1. Проверка здоровья</h3>
            <div class='example'>
                GET /api/Messenger/health
                -H 'X-Api-Key: your-api-key'
            </div>

            <h3>2. Получить статус чата с ботом</h3>
            <div class='example'>
                GET /api/Messenger/status?user_huid=USER_HUID
                -H 'Authorization: Bearer YOUR_JWT_TOKEN'
                -H 'X-Api-Key: your-api-key'
            </div>

            <h3>3. Отправить команду /help</h3>
            <div class='example'>
                POST /api/Messenger/command
                -H 'X-Api-Key: your-api-key'
                -H 'Content-Type: application/json'
                {
                    ""command"": ""/help"",
                    ""sender"": ""User123""
                }
            </div>

            <h3>4. Создать заявку</h3>
            <div class='example'>
                POST /api/Messenger/command
                -H 'X-Api-Key: your-api-key'
                -H 'Content-Type: application/json'
                {
                    ""command"": ""/create Не работает принтер Принтер не печатает, ошибка 49"",
                    ""sender"": ""User123""
                }
            </div>

            <h3>5. Проверить статус заявки</h3>
            <div class='example'>
                POST /api/Messenger/command
                -H 'X-Api-Key: your-api-key'
                -H 'Content-Type: application/json'
                {
                    ""command"": ""/status 12345"",
                    ""sender"": ""User123""
                }
            </div>

            <h3>6. Получить список заявок</h3>
            <div class='example'>
                POST /api/Messenger/command
                -H 'X-Api-Key: your-api-key'
                -H 'Content-Type: application/json'
                {
                    ""command"": ""/issues"",
                    ""sender"": ""User123""
                }
            </div>

            <h2>Доступные команды бота</h2>
            <div class='info'>
                <p><strong>/help</strong> - Показать справку</p>
                <p><strong>/start</strong> - Начать работу</p>
                <p><strong>/status [номер]</strong> - Статус заявки</p>
                <p><strong>/create [тема] [описание]</strong> - Создать новую заявку</p>
                <p><strong>/issues</strong> - Список ваших заявок</p>
                <p><strong>/stop</strong> - Завершить сессию</p>
            </div>

            <h2>Конфигурация в appsettings.json</h2>
            <div class='highlight'>
                <code>
                    {
                    ""ApiKey"": ""your-api-key"",
                    ""ExpressSettings"": {
                        ""ApiUrl"": ""https://x.ar-management.ru"",
                        ""SecKey"": ""your-sec-key"",
                        ""BotId"": ""your-bot-id""
                    },
                    ""RedmineSettings"": {
                        ""BaseUrl"": ""https://your-redmine.com"",
                        ""ApiToken"": ""your-redmine-token""
                    }
                    }
                </code>
            </div>

            <h2>Переменные окружения для Docker</h2>
            <div class='highlight'>
                <p><code>EXPRESS_API_URL</code> - URL Express API</p>
                <p><code>EXPRESS_API_KEY</code> - API ключ для Gateway</p>
                <p><code>REDMINE_BASE_URL</code> - URL Redmine</p>
                <p><code>REDMINE_API_TOKEN</code> - API токен Redmine</p>
                <p><code>PORT</code> - Порт сервера (5000)</p>
            </div>

            <div class='success'>
                <p><strong>Быстрая проверка:</strong></p>
                <div class='highlight'>
                    <code>curl -X GET ""http://localhost:5000/api/Messenger/health"" -H ""X-Api-Key: your-api-key""</code>
                </div>
            </div>
        </div>
        ";
    });
}

app.UseCors(policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
});

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    service = "Express Gateway API"
}));

app.Run();
