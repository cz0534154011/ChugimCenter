using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using ChugimCenter.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace ChugimCenter.API.Extensions;

/// <summary>ההגדרות של שכבת ה-API, מחולקות לפי נושא, כדי ש-Program.cs יישאר קצר וקריא.</summary>
public static class ApiServiceExtensions
{
    public const string ClientCorsPolicy = "Client";

    /// <summary>Controllers, ‏enum כטקסט ב-JSON, ותשובת 400 אחידה לשגיאות קלט.</summary>
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers()
                .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // [ApiController] בודק את ModelState לפני שה-Action רץ. כאן קובעים שהתשובה תהיה במבנה האחיד,
        // עם פירוט השגיאות לפי שדה (מתוך ה-Data Annotations שעל ה-DTOs)
        services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                return new BadRequestObjectResult(ErrorResponseFactory.Create(
                    context.HttpContext, StatusCodes.Status400BadRequest, "הנתונים שנשלחו אינם תקינים", errors));
            });

        return services;
    }

    /// <summary>
    /// הגדרות ה-JWT דרך IOptions, ובדיקת ה-token בכל בקשה.
    /// אם המפתח חסר או קצר מדי, השרת לא עולה בכלל, עם הודעה ברורה.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(s => s.Key.Length >= 32, "Jwt:Key is missing or shorter than 32 characters. Set it with User Secrets.")
            .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer) && !string.IsNullOrWhiteSpace(s.Audience),
                "Jwt:Issuer and Jwt:Audience are required.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        // אותן הגדרות שבהן ה-token נוצר (JwtTokenGenerator) משמשות לבדיקה שלו
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };

                // 401 ו-403 מוחזרים באותו מבנה אחיד כמו כל שאר השגיאות
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        return ErrorResponseFactory.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, "יש להתחבר למערכת");
                    },
                    OnForbidden = context =>
                        ErrorResponseFactory.WriteAsync(context.HttpContext, StatusCodes.Status403Forbidden, "אין לך הרשאה לפעולה זו")
                };
            });

        services.AddAuthorization();
        return services;
    }

    /// <summary>Swagger שמתעד את כל ה-endpoints, עם כפתור Authorize להדבקת ה-token.</summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ChugimCenter API",
                Version = "v1",
                Description = "רישום לחוגים במתנ\"ס. להתחברות: POST /api/auth/login, ואז Authorize עם ה-token."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "הדביקי כאן את ה-token מתשובת ההתחברות (בלי המילה Bearer)."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        return services;
    }

    /// <summary>CORS: רק כתובות הלקוח שמוגדרות בקונפיגורציה רשאיות לפנות לשרת מהדפדפן.</summary>
    public static IServiceCollection AddClientCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options => options.AddPolicy(ClientCorsPolicy, policy =>
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()));

        return services;
    }
}
