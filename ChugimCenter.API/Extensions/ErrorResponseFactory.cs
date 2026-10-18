using ChugimCenter.Application.DTOs.Common;
using Microsoft.AspNetCore.WebUtilities;

namespace ChugimCenter.API.Extensions;

/// <summary>
/// בונה את תשובת השגיאה האחידה (ErrorResponse) במקום אחד.
/// משמש את תרגום ה-Result, את שגיאות הקלט (400), את 401 ו-403, ובשלב 7 גם את ה-Middleware של השגיאות.
/// </summary>
public static class ErrorResponseFactory
{
    /// <summary>המפתח שבו ה-Middleware של שלב 7 ישמור את ה-CorrelationId של הבקשה.</summary>
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

    // עד שלב 7 משמש המזהה הפנימי של ASP.NET לכל בקשה
    private static string GetCorrelationId(HttpContext context)
        => context.Items[CorrelationIdKey] as string ?? context.TraceIdentifier;
}
