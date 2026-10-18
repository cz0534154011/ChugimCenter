using ChugimCenter.API.Extensions;
using ChugimCenter.Application;
using ChugimCenter.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

// סדר ה-pipeline חשוב. בשלב 7 יתווספו כאן, ראשונים, ה-Middleware של השגיאות ושל ה-CorrelationId.
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
