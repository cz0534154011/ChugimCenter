# ChugimCenter – אתר רישום לחוגים

שרת ASP.NET Core Web API לאתר רישום לחוגי בנות במתנ"ס קהילתי אחד: ציור, דרמה, התעמלות, מוזיקה ועוד.

## תיאור המערכת

המערכת מאפשרת למשתתפות להירשם לאתר, לצפות בחוגים ולהירשם לקבוצות. מדריכות רואות את הקבוצות שלהן ואת רשימות התלמידות, והמנהלת מנהלת תחומים, קבוצות, מדריכות, רישומים ותשלומים. יש שלושה תפקידים: Admin (מנהלת), Instructor (מדריכה) ו-Member (משתתפת). כל הרשאה נבדקת בשרת, לפי התפקיד שבתוך ה-JWT החתום, ובדיקת "הקבוצה שלך" נעשית מול בסיס הנתונים.

ארבע ישויות:

- תחום (`Category`): סוג החוג, למשל ציור או דרמה.
- משתמשת (`User`): מנהלת, מדריכה או משתתפת. אצל משתתפת החשבון הוא של הילדה, עם פרטי ההורה.
- קבוצת חוג (`CourseGroup`): תאריך התחלה, מספר מפגשים (אחד בשבוע), שעה, טווח גילאים, מחיר ומספר מקומות. היום בשבוע ותאריך הסיום מחושבים.
- רישום לחוג (`Enrollment`): "משתתפת מסוימת רשומה לקבוצה מסוימת", עם סטטוס (פעיל או מבוטל) וסימון תשלום.

קשרים: תחום ← קבוצות, ומדריכה ← קבוצות (one-to-many); משתתפות ↔ קבוצות דרך טבלת הרישומים (many-to-many).

## המשאב המוגבל והתחרות עליו

המשאב המוגבל הוא **מקום בקבוצת חוג**. התרחיש שהמערכת חייבת למנוע: "שתי משתתפות לחצו 'רישום לחוג' בו-זמנית על המקום האחרון, שתיהן קיבלו אישור, ואסור היה ששתיהן יקבלו".

הפתרון הוא optimistic concurrency:

- גרסה: לקבוצה יש שדה `Version` מסוג `Guid`, שמוגדר כ-concurrency token (`IsConcurrencyToken` ב-Fluent API). הערך מתחלף אוטומטית בכל שמירה, בדריסה של `SaveChangesAsync` ב-`AppDbContext`.
- מספר הנרשמות על הקבוצה: `EnrolledCount` נשמר על שורת הקבוצה. כך כל רישום מעדכן את אותה שורה, ושתי הרשמות מתחרות מתנגשות עליה. בלי זה, כל בקשה רק הייתה מוסיפה שורה אחרת לטבלת הרישומים, וההתנגשות לא הייתה מזוהה.
- הזרימה: ה-Service בודק את כל הכללים (קבוצה פעילה, גיל, חפיפה בזמן, מקום פנוי), יוצר את הרישום ומגדיל את `EnrolledCount` באותה שמירה. EF מוסיף לפקודת ה-UPDATE את התנאי `WHERE "Version" = <הגרסה שנקראה>`.
- ההתנגשות: אם בקשה אחרת שמרה בינתיים, אף שורה לא מתעדכנת ו-EF זורק `DbUpdateConcurrencyException`. ה-Repository (שכבת Infrastructure) מתרגם אותה ל-`ConcurrencyConflictException` שלנו, כדי ששכבת Application לא תכיר את EF. ה-Service תופס אותה, כותב ללוג ברמת Warning ומחזיר **409** עם ההודעה "המקום נתפס על ידי משתמשת אחרת. נסי שוב.".
- רשתות ביטחון בבסיס הנתונים: אינדקס ייחודי מסונן על "משתתפת + קבוצה" לרישומים פעילים בלבד, ו-Check Constraint שמבטיח `EnrolledCount <= Capacity`.
- ביטול: ביטול רישום משנה את הסטטוס ל"מבוטל" ומקטין את `EnrolledCount`, באותה שמירה ועם אותה הגנת גרסה. השורה לא נמחקת.

שני מקרים שונים מחזירים 409:

1. קבוצה מלאה: אם בזמן הבקשה כבר אין מקום, ה-Service מחזיר 409 "אין מקומות פנויים בקבוצה" עוד לפני השמירה. זה מה שרואים כשמנסים להירשם ב-Swagger לקבוצה שהתמלאה.
2. תחרות אמיתית: שתי בקשות קראו את הקבוצה כשעוד היה בה מקום, והשנייה נכשלת בשמירה בגלל הגרסה. את זה אי אפשר ליצור ידנית ב-Swagger, ולכן יש לו בדיקה ייעודית.

ההוכחה: הבדיקה `CourseGroupConcurrencyTests` בפרויקט `ChugimCenter.Tests`. שני `DbContext` נפרדים קוראים את אותה קבוצה (קבוצה 3 מנתוני ה-Seed, שנשאר בה מקום אחד), ושניהם מוסיפים רישום ומגדילים את מספר הנרשמות. הראשון שומר בהצלחה, והשני מקבל `DbUpdateConcurrencyException`. בדיקה שנייה עושה את אותו הדבר דרך ה-Repository ומקבלת `ConcurrencyConflictException`. בשתיהן נבדק בסוף שנשמר רק רישום אחד.

הבדיקה רצה על SQLite בזיכרון: בסיס נתונים אמיתי, שמקבל מ-EF את אותה פקודת UPDATE עם תנאי WHERE על הגרסה כמו PostgreSQL. המבנה ונתוני ה-Seed נוצרים מאותן הגדרות Fluent API של הפרויקט. כך הבדיקה רצה בלי Docker ובלי סיסמה, בכל מחשב.

## ארכיטקטורה

הפרויקט בנוי לפי Clean Architecture, בחמישה פרויקטים בתוך `ChugimCenter.sln`:

| פרויקט | תפקיד | מקביל במבנה הקלאסי |
|---|---|---|
| `ChugimCenter.Domain` | ישויות ו-Enums. לא תלוי בשום פרויקט או חבילה | Core |
| `ChugimCenter.Application` | DTOs, ממשקים, Services, פרופילי AutoMapper, Result | Core + Service |
| `ChugimCenter.Infrastructure` | DbContext, Fluent API, Migrations, Repositories, BCrypt, JWT | Data |
| `ChugimCenter.API` | Controllers, Middleware, Program.cs, הגדרות | API |
| `ChugimCenter.Tests` | xUnit ו-Moq, ובדיקת התחרות | – |

כיוון התלויות: Application ← Domain; Infrastructure ← Application; API ← Application ו-Infrastructure (רק לרישום השירותים ב-Program.cs). Domain לא מכיר את EF Core, וה-Controllers לא מכירים את ה-DbContext: הם עוברים דרך Services.

## טכנולוגיות

- שרת: ASP.NET Core Web API על .NET 10.
- בסיס נתונים: PostgreSQL 16, עם EF Core 10 (Npgsql), בגישת Code-First ושתי Migrations: `Init` ו-`AddPayment`.
- אימות: JWT עם claims של מזהה, שם ותפקיד. סיסמאות מוצפנות ב-BCrypt.
- מיפוי: AutoMapper 14.0.0. זו הגרסה האחרונה ברישיון MIT, ויש בה חולשה ידועה (CVE-2026-32933). היא מושתקת רק עבור החולשה הזו, ב-`Directory.Build.props`. אין סיכון אצלנו, כי ממפים רק DTOs שטוחים.
- לוגים: NLog, עם `nlog.config`.
- בדיקות: xUnit, ‏Moq, ו-SQLite בזיכרון לבדיקת התחרות.
- תיעוד: Swagger (Swashbuckle) עם כפתור Authorize.

## הרצה מקומית

דרישות: .NET SDK 10, ‏Docker Desktop, והכלי dotnet-ef:

```
dotnet tool install --global dotnet-ef
```

שלב 1 – הרמת PostgreSQL ב-Docker (במקום `<password>` בוחרים סיסמה):

```
docker run --name chugim-pg -e POSTGRES_PASSWORD=<password> -p 5432:5432 -v chugim-pgdata:/var/lib/postgresql/data -d postgres:16
```

שלב 2 – סודות ב-User Secrets. אין אף סוד בקוד או ב-repo. מתוך התיקייה של `ChugimCenter.sln`:

```
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=ChugimCenterDb;Username=postgres;Password=<password>" --project ChugimCenter.API
dotnet user-secrets set "Jwt:Key" "<מפתח אקראי באורך 32 תווים לפחות>" --project ChugimCenter.API
```

יצירת מפתח אקראי ב-PowerShell:

```
$bytes = New-Object byte[] 64; [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes); [Convert]::ToBase64String($bytes)
```

שלב 3 – יצירת בסיס הנתונים: הפקודה יוצרת את בסיס הנתונים, את כל הטבלאות ואת נתוני הדמו. אין צורך בשום פעולה ידנית נוספת בבסיס הנתונים.

```
dotnet ef database update -p ChugimCenter.Infrastructure -s ChugimCenter.API
```

שלב 4 – הרצת השרת:

```
dotnet run --project ChugimCenter.API --launch-profile https
```

Swagger נפתח ב-`https://localhost:7200/swagger` (או `http://localhost:5212/swagger` בפרופיל http).

התחברות ב-Swagger: שולחים `POST /api/auth/login`, מעתיקים מהתשובה את ה-`token` (בלי מירכאות), לוחצים Authorize ומדביקים אותו בלי המילה Bearer.

הרצת הבדיקות (לא דורשת Docker או סיסמה):

```
dotnet test
```

## משתמשות דמו

| תפקיד | שם | אימייל | סיסמה |
|---|---|---|---|
| מנהלת (Admin) | רבקה לוי | admin@example.com | Admin1234! |
| מדריכה (Instructor) | שרה כהן | sara@example.com | Teacher1234! |
| מדריכה (Instructor) | מרים פרידמן | miriam@example.com | Teacher1234! |
| משתתפת (Member) | חנה גולד | chana@example.com | Member1234! |
| משתתפת (Member) | אסתר רוזן | esther@example.com | Member1234! |
| משתתפת (Member) | לאה שטרן | leah@example.com | Member1234! |

בנתוני הדמו יש ארבעה תחומים וחמש קבוצות. לקבוצה 3 ("דרמה גילאי 8–11", המדריכה מרים) יש 2 מקומות, וחנה כבר רשומה אליה.

תרחיש הדגמה ב-Swagger:

1. אסתר מתחברת ושולחת `POST /api/enrollments` עם `{ "courseGroupId": 3 }`: מקבלת 201, והקבוצה מלאה.
2. לאה מתחברת ושולחת את אותה בקשה: מקבלת 409 "אין מקומות פנויים בקבוצה".
3. מרים מתחברת ושולחת `GET /api/course-groups/3/students`: מקבלת 200 עם חנה ואסתר. עם קבוצה 1, שאינה שלה, היא מקבלת 403.

## טיפול בשגיאות ולוגים

כל שגיאה מוחזרת במבנה JSON אחיד (`ErrorResponse`), והמערכת לא קורסת:

```json
{
  "status": 409,
  "title": "Conflict",
  "message": "אין מקומות פנויים בקבוצה",
  "errors": null,
  "correlationId": "52dd83d31073405a860113f7835054a5",
  "timestamp": "2026-10-19T00:00:00Z"
}
```

- שגיאות צפויות: ה-Service מחזיר `Result` עם סוג שגיאה, ו-`ResultExtensions` מתרגם אותו לסטטוס: 400, ‏401, ‏403, ‏404 או 409.
- שגיאות קלט: Data Annotations על ה-DTOs מחזירות 400, עם `errors` לפי שדה.
- 401 ו-403: מוחזרים באותו מבנה, בעברית.
- שגיאות לא צפויות: `ErrorHandlingMiddleware`, הראשון ב-pipeline, מחזיר 500 עם הודעה כללית ומזהה לבירור, וכותב ללוג ברמת Error.

לוגים (NLog):

- כל בקשה מקבלת מזהה מ-`CorrelationIdMiddleware`. המזהה חוזר בכותרת `X-Correlation-ID`, בתשובת השגיאה ובכל שורות הלוג של הבקשה.
- רמות: Information לכל בקשה, Warning להתנגשות על מקום, Error לחריגה, Debug בסביבת פיתוח.
- לא נכתבים ללוג סיסמאות, tokens או תוכן של בקשת הרשמה.
- הקובץ: `ChugimCenter.API/bin/Debug/net10.0/logs/chugim-<תאריך>.log`, וגם הפלט במסוף.

## נקודות הכניסה (Endpoints)

| Method | נתיב | מי רשאי |
|---|---|---|
| POST | `/api/auth/register` | כולם (הרשמת משתתפת) |
| POST | `/api/auth/login` | כולם |
| GET | `/api/users/me` | כל משתמשת מחוברת |
| GET | `/api/users/me/enrollments` | משתתפת ("החוגים שלי") |
| GET | `/api/users/me/course-groups` | מדריכה ("הקבוצות שלי") |
| GET | `/api/users`, ‏`/api/users/{id}` | מנהלת |
| POST | `/api/instructors` | מנהלת |
| GET | `/api/categories`, ‏`/api/categories/{id}` | כולם |
| POST, PUT, PATCH, DELETE | `/api/categories`, ‏`/api/categories/{id}` | מנהלת |
| GET | `/api/course-groups` (סינון, מיון ו-pagination), ‏`/api/course-groups/{id}` | כולם |
| POST, PUT, PATCH, DELETE | `/api/course-groups`, ‏`/api/course-groups/{id}` | מנהלת |
| GET | `/api/course-groups/{id}/students` | המדריכה של הקבוצה, מנהלת |
| POST | `/api/enrollments` | משתתפת |
| GET | `/api/enrollments/{id}` | בעלת הרישום, מנהלת |
| DELETE | `/api/enrollments/{id}` (ביטול) | בעלת הרישום, מנהלת |
| GET | `/api/enrollments` | מנהלת |
| PUT | `/api/enrollments/{id}/payment` | מנהלת |
