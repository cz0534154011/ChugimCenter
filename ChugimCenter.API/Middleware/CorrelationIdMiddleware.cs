using System.Diagnostics;
using ChugimCenter.API.Extensions;

namespace ChugimCenter.API.Middleware;

/// <summary>
/// נותן לכל בקשה מזהה (CorrelationId), ומצרף אותו לכל שורות הלוג של הבקשה ולכותרת התשובה.
/// בסוף כל בקשה נכתבת שורת Information אחת: method, נתיב, סטטוס וזמן.
/// יושב אחרי ErrorHandlingMiddleware ב-pipeline (טיפול בשגיאות ראשון, לפי הוראות המרצה).
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-ID";
    private const int MaxLength = 64;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context.Request);

        // ErrorResponseFactory קורא את המזהה מכאן, וכך הוא מופיע בכל תשובת שגיאה
        context.Items[ErrorResponseFactory.CorrelationIdKey] = correlationId;

        // הכותרת נוספת רגע לפני שליחת התשובה, כדי שלא תימחק אם ה-Middleware של השגיאות מנקה את התשובה
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // כל שורת לוג שנכתבת בתוך ה-scope, מכל שכבה, מקבלת את המזהה (ב-nlog.config: ${scopeproperty:CorrelationId})
        using (BeginCorrelationScope(logger, correlationId))
        {
            // רק הנתיב, בלי ה-query string, ובלי כותרות או תוכן הבקשה
            var method = context.Request.Method;
            var path = context.Request.Path.Value;
            logger.LogDebug("Request started: {Method} {Path}", method, path);

            var startTime = Stopwatch.GetTimestamp();
            try
            {
                await next(context);
            }
            catch (Exception)
            {
                // טיפול השגיאות יושב לפני זה (מבחוץ), ולכן כאן הסטטוס הסופי עוד לא נקבע, והוא עדיין 200.
                // לא כותבים סטטוס שגוי: מתעדים שהבקשה נכשלה, וזורקים את החריגה הלאה בלי לטפל בה.
                // ה-500 ושורת ה-Error נכתבים ב-ErrorHandlingMiddleware
                logger.LogInformation("HTTP {Method} {Path} failed with an unhandled exception after {ElapsedMs:0} ms",
                    method, path, Stopwatch.GetElapsedTime(startTime).TotalMilliseconds);
                throw;
            }

            logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs:0} ms",
                method, path, context.Response.StatusCode, Stopwatch.GetElapsedTime(startTime).TotalMilliseconds);
        }
    }

    /// <summary>
    /// פותח scope שבו כל שורת לוג מקבלת את ה-CorrelationId (ב-nlog.config: ${scopeproperty:CorrelationId}).
    /// משמש גם את ErrorHandlingMiddleware, כדי ששורת ה-Error תכלול את אותו מזהה.
    /// </summary>
    public static IDisposable? BeginCorrelationScope(ILogger logger, string correlationId)
        => logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });

    // לקוח רשאי לשלוח מזהה משלו (למשל כדי לעקוב אחרי פעולה מהדפדפן ועד השרת).
    // מקבלים אותו רק אם הוא קצר ומכיל אותיות, ספרות ומקפים, כדי שאי אפשר יהיה "להזריק" טקסט ללוג
    private static string GetOrCreateCorrelationId(HttpRequest request)
    {
        var fromHeader = request.Headers[HeaderName].ToString();
        var isValid = fromHeader.Length is > 0 and <= MaxLength
            && fromHeader.All(c => char.IsAsciiLetterOrDigit(c) || c == '-');

        return isValid ? fromHeader : Guid.NewGuid().ToString("N");
    }
}
