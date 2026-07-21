namespace ExpressGateway.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private const string ApiKeyHeader = "X-Api-Key";

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        if (IsPublicEndpoint(path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey))
        {
            _logger.LogWarning("Missing API key for: {Path}", path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "API key missing",
                errorCode = "MISSING_API_KEY",
                timestamp = DateTime.UtcNow
            });
            return;
        }

        var expectedKey = _configuration["ApiKey"];
        
        if (string.IsNullOrEmpty(expectedKey))
        {
            _logger.LogError("ApiKey not configured");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Server configuration error",
                errorCode = "API_KEY_NOT_CONFIGURED",
                timestamp = DateTime.UtcNow
            });
            return;
        }

        if (!expectedKey.Equals(apiKey.ToString()))
        {
            _logger.LogWarning("Invalid API key for: {Path}", path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid API key",
                errorCode = "INVALID_API_KEY",
                timestamp = DateTime.UtcNow
            });
            return;
        }

        await _next(context);
    }

    private static bool IsPublicEndpoint(string path)
    {
        return path.StartsWith("/swagger") ||
               path.StartsWith("/health") ||
               path == "/" ||
               path.StartsWith("/info");
    }
}