using ChugimCenter.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// מחרוזת החיבור נטענת מהקונפיגורציה (בפיתוח: User Secrets), ולא כתובה בקוד
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' was not found. Set it with User Secrets.");

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
