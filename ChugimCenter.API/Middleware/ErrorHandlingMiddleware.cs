using ChugimCenter.API.Extensions;

namespace ChugimCenter.API.Middleware;

/// <summary>
/// תופס כל חריגה לא צפויה בהמשך ה-pipeline, כותב אותה ללוג ברמת Error,
/// ומחזיר 500 במבנה השגיאה האחיד. כך השרת לא קורס, והמשתמשת מקבלת מזהה לבירור.
/// שגיאות צפויות (404, 409 וכו') לא מגיעות לכאן: הן חוזרות כ-Result מה-Services.
/// ראשון ב-pipeline (לפי הוראות המרצה), ולכן עוטף את כל השלבים שאחריו, כולל CorrelationIdMiddleware.
/// </summary>
public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // הלקוח סגר את החיבור באמצע (למשל סגר את הדף). זו לא תקלה בשרת, ואין למי להחזיר תשובה
            logger.LogDebug("The request was cancelled by the client");
        }
        catch (Exception ex)
        {
            // כאן תופסים Exception כללי בכוונה: זו רשת הביטחון האחרונה, לכל מה שלא טופל במקום אחר
            var correlationId = ErrorResponseFactory.GetCorrelationId(context);

            // ה-scope של CorrelationIdMiddleware כבר נסגר כאן, כי הוא יושב בתוך ה-Middleware הזה.
            // פותחים scope עם אותו מזהה (שנשמר ב-HttpContext.Items), כדי ששורת ה-Error תכלול אותו
            using (CorrelationIdMiddleware.BeginCorrelationScope(logger, correlationId))
            {
                logger.LogError(ex, "Unhandled exception while processing {Method} {Path}",
                    context.Request.Method, context.Request.Path.Value);
            }

            // אם חלק מהתשובה כבר נשלח, אי אפשר להחליף אותה בתשובת שגיאה
            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            await ErrorResponseFactory.WriteAsync(context, StatusCodes.Status500InternalServerError,
                $"אירעה שגיאה. מזהה לבירור: {correlationId}");
        }
    }
}
