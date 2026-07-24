using ExpressGateway.Middleware;
using ExpressGateway.Services;
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
builder.Services.AddScoped<IExpressService, ExpressService>();

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
            
            <div class='success'>
                <p><strong>Проверка через curl:</strong></p>
                <div class='highlight'>
                    <code>curl -X GET ""http://localhost:5000/api/Messenger/ping"" -H ""X-Api-Key: your-api-key-here""</code>
                </div>
            </div>
            
            <h2>Доступные эндпоинты</h2>
            <div class='highlight'>
                <p><span class='method'>POST</span> <span class='endpoint'>/api/Messenger/send</span> - Отправить сообщение в чат</p>
                <p><span class='method'>POST</span> <span class='endpoint'>/api/Messenger/send-default</span> - Отправить сообщение в группу по умолчанию</p>
                <p><span class='method'>GET</span> <span class='endpoint'>/api/Messenger/chats</span> - Получить список чатов</p>
                <p><span class='method'>GET</span> <span class='endpoint'>/api/Messenger/ping</span> - Проверка доступности API</p>
                <p><span class='method'>GET</span> <span class='endpoint'>/health</span> - Health check сервиса</p>
            </div>

            <h2>Примеры запросов</h2>
            
            <h3>Отправка сообщения</h3>
            <div class='example'>
                POST /api/Messenger/send<br/>
                {<br/>
                &nbsp;&nbsp;""chatId"": ""*****"",<br/>
                &nbsp;&nbsp;""text"": ""Привет, мир!""<br/>
                }
            </div>

            <h3>Отправка в группу по умолчанию</h3>
            <div class='example'>
                POST /api/Messenger/send-default<br/>
                {<br/>
                &nbsp;&nbsp;""text"": ""Сообщение в группу по умолчанию""<br/>
                }
            </div>

            <h3>Получение списка чатов</h3>
            <div class='example'>
                GET /api/Messenger/chats?limit=10&offset=0
            </div>

            <h2>Групповой ChatId по умолчанию</h2>
            <div class='info'>
                <p><strong>Chat ID:</strong> <code>*****</code></p>
                <p><small>Этот ID используется в методе send-default</small></p>
            </div>

            <h2>Переменные окружения</h2>
            <div class='highlight'>
                <p><code>EXPRESS_API_URL</code> - URL для вызовов к Express API</p>
                <p><code>EXPRESS_API_KEY</code> - API ключ для аутентификации</p>
                <p><code>PORT</code> - Порт сервера (по умолчанию 3000)</p>
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
