namespace ExpressGateway.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Error { get; set; }
    public string? ErrorCode { get; set; }
}
