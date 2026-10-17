namespace ChugimCenter.Application.DTOs.Common;

/// <summary>מבנה השגיאה האחיד שהשרת מחזיר בכל שגיאה.</summary>
public class ErrorResponse
{
    public int Status { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    // שגיאות לפי שדה, רק בשגיאת ולידציה (400)
    public IDictionary<string, string[]>? Errors { get; init; }

    public string? CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
