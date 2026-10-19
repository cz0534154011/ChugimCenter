# בניית השרת לתמונת Docker, להעלאה ל-Render.
# שני שלבים: בשלב הראשון ה-SDK המלא בונה ומפרסם, ובשלב השני רק ה-runtime מריץ - תמונה קטנה יותר.

# ---------- שלב 1: בנייה ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# קודם רק קבצי הפרויקטים, ו-restore. כל עוד החבילות לא השתנו, Docker משתמש בשכבה הזו מהזיכרון
COPY Directory.Build.props ./
COPY ChugimCenter.Domain/ChugimCenter.Domain.csproj ChugimCenter.Domain/
COPY ChugimCenter.Application/ChugimCenter.Application.csproj ChugimCenter.Application/
COPY ChugimCenter.Infrastructure/ChugimCenter.Infrastructure.csproj ChugimCenter.Infrastructure/
COPY ChugimCenter.API/ChugimCenter.API.csproj ChugimCenter.API/
RUN dotnet restore ChugimCenter.API/ChugimCenter.API.csproj

# עכשיו שאר הקוד (bin, obj ו-.vs מסוננים ב-.dockerignore), ופרסום של פרויקט ה-API בלבד.
# פרויקט הבדיקות לא נכנס לתמונה
COPY . .
RUN dotnet publish ChugimCenter.API/ChugimCenter.API.csproj -c Release -o /app/publish --no-restore

# ---------- שלב 2: הרצה ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# תיקיית הלוגים של NLog. השרת רץ כמשתמש רגיל (לא root), ולכן צריך לתת לו הרשאת כתיבה אליה
RUN mkdir -p /app/logs && chown $APP_UID /app/logs

# השרת מאזין ב-HTTP על פורט 8080. ה-HTTPS מטופל ב-Render לפני שהבקשה מגיעה לכאן
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "ChugimCenter.API.dll"]
