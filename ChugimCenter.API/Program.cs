using ChugimCenter.API.Extensions;
using ChugimCenter.API.Middleware;
using ChugimCenter.Application;
using ChugimCenter.Infrastructure;
using NLog;
using NLog.Web;

// NLog נטען לפני בניית השרת (מתוך nlog.config), כדי שגם תקלה בזמן העלייה תיכתב ללוג
var startupLogger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog מחליף את ספקי הלוג המובנים. הקוד בכל השכבות ממשיך לכתוב דרך ILogger הרגיל.
    // RemoveLoggerFactoryFilter = false: הרמות נקבעות ב-appsettings (Logging:LogLevel), לפי הסביבה
    builder.Logging.ClearProviders();
    builder.Host.UseNLog(new NLogAspNetCoreOptions { RemoveLoggerFactoryFilter = false });

    // מחרוזת החיבור נטענת מהקונפיגורציה (בפיתוח: User Secrets), ולא כתובה בקוד
    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' was not found. Set it with User Secrets.");

    // רישום השירותים (Dependency Injection). כל שכבה רושמת את המחלקות שלה
    builder.Services.AddApiControllers();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(connectionString);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddSwaggerWithJwt();
    builder.Services.AddClientCors(builder.Configuration);

    var app = builder.Build();

    // סדר ה-pipeline חשוב: כל בקשה עוברת את השורות מלמעלה למטה, והתשובה חוזרת מלמטה למעלה.
    // 1. טיפול בשגיאות ראשון (לפי הוראות המרצה): עוטף את כל ה-pipeline, ולכן תופס חריגה לא צפויה מכל שלב שאחריו ומחזיר 500 אחיד.
    // 2. CorrelationId: נותן מזהה לבקשה ומצרף אותו לכל שורות הלוג שלה.
    //    המזהה נשמר גם ב-HttpContext.Items, ולכן טיפול השגיאות, שעוטף אותו מבחוץ, משתמש בו בשורת ה-Error ובתשובת ה-500.
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();

    // Swagger פעיל בכל סביבה, כדי שאפשר יהיה לבדוק את השרת גם אחרי ההעלאה ל-Render
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DocumentTitle = "ChugimCenter API");

    app.UseHttpsRedirection();
    app.UseCors(ApiServiceExtensions.ClientCorsPolicy);

    // קודם מזהים מי פונה (Authentication), ורק אחר כך בודקים מה מותר לה (Authorization)
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
// HostAbortedException: כלי ה-Migrations (dotnet ef) עוצר את השרת בכוונה אחרי הבנייה. זו לא תקלה
catch (Exception ex) when (ex is not HostAbortedException)
{
    startupLogger.Error(ex, "The server stopped because of an exception during startup");
    throw;
}
finally
{
    // כותב ללוג את מה שעוד ממתין בזיכרון, לפני שהתהליך נסגר
    LogManager.Shutdown();
}
