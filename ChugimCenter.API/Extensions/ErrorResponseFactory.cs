using ChugimCenter.Application.DTOs.Common;
using Microsoft.AspNetCore.WebUtilities;

namespace ChugimCenter.API.Extensions;

/// <summary>
/// בונה את תשובת השגיאה האחידה (ErrorResponse) במקום אחד.
/// משמש את תרגום ה-Result, את שגיאות הקלט (400), את 401 ו-403, ואת ה-Middleware של השגיאות (500).
/// </summary>
public static class ErrorResponseFactory
{
    /// <summary>המפתח שבו CorrelationIdMiddleware שומר את ה-CorrelationId של הבקשה.</summary>
    public const string CorrelationIdKey = "CorrelationId";

    public static ErrorResponse Create(
        HttpContext context, int status, string message, IDictionary<string, string[]>? errors = null)
        => new()
        {
            Status = status,
            Title = ReasonPhrases.GetReasonPhrase(status),
            Message = message,
            Errors = errors,
            CorrelationId = GetCorrelationId(context)
        };

    /// <summary>כותב תשובת שגיאה אחידה ישירות לתשובה, מחוץ ל-Controller (‏401, ‏403, ‏500).</summary>
    public static Task WriteAsync(HttpContext context, int status, string message)
    {
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(Create(context, status, message));
    }

    // המזהה ש-CorrelationIdMiddleware יצר. אם הוא חסר, משמש המזהה הפנימי של ASP.NET לבקשה
    public static string GetCorrelationId(HttpContext context)
        => context.Items[CorrelationIdKey] as string ?? context.TraceIdentifier;
}
