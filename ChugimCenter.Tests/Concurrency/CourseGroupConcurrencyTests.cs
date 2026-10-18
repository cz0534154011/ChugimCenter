using ChugimCenter.Application.Common.Exceptions;
using ChugimCenter.Domain.Entities;
using ChugimCenter.Domain.Enums;
using ChugimCenter.Infrastructure.Persistence;
using ChugimCenter.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ChugimCenter.Tests.Concurrency;

/// <summary>
/// ההוכחה שהתחרות על מקום בקבוצה מטופלת (חלק ג' בהוראות).
/// שני DbContext נפרדים מדמים שתי בקשות בו-זמניות של שתי משתתפות על המקום האחרון.
/// בסיס הנתונים הוא SQLite בזיכרון: בסיס נתונים אמיתי, שמקבל מ-EF את אותה פקודת UPDATE
/// עם תנאי WHERE על הגרסה, ורץ בלי Docker ובלי סיסמה. המבנה ונתוני ה-Seed נוצרים
/// מאותן הגדרות Fluent API של הפרויקט. xUnit יוצר מופע חדש לכל בדיקה, ולכן לכל בדיקה בסיס נתונים נקי.
/// </summary>
public sealed class CourseGroupConcurrencyTests : IDisposable
{
    // קבוצה 3 מה-Seed (דרמה): 2 מקומות ונרשמת אחת (חנה), כלומר נשאר מקום אחרון
    private const int DemoGroupId = 3;
    private const int ChanaId = 4;
    private const int EstherId = 5;
    private const int LeahId = 6;

    // בסיס נתונים בזיכרון קיים כל עוד החיבור פתוח, ונמחק כשהחיבור נסגר
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<AppDbContext> _options;

    public CourseGroupConcurrencyTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task TwoContexts_SaveSameGroup_FirstSucceedsSecondThrows()
    {
        // Arrange: שתי בקשות נפרדות, של אסתר ושל לאה, כל אחת עם DbContext משלה.
        // שתיהן קוראות את הקבוצה לפני ששמרו, ולכן שתיהן רואות את אותה גרסה ואת אותו מקום פנוי
        await using var estherContext = CreateContext();
        await using var leahContext = CreateContext();
        var esthersGroup = await TakeLastSeatAsync(estherContext, EstherId);
        var leahsGroup = await TakeLastSeatAsync(leahContext, LeahId);
        Assert.Equal(esthersGroup.Version, leahsGroup.Version);

        // Act + Assert: אסתר שומרת ראשונה ומצליחה, והגרסה מתחלפת.
        // לאה שומרת אחריה עם הגרסה הישנה: אף שורה לא מתעדכנת, ו-EF זורק חריגה
        await estherContext.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => leahContext.SaveChangesAsync());

        await AssertOnlyEstherGotTheSeatAsync();
    }

    [Fact]
    public async Task TwoContexts_SaveThroughRepository_SecondThrowsConcurrencyConflict()
    {
        // Arrange: אותו תרחיש, אבל השמירה עוברת דרך ה-Repository שלנו, כמו בשרת
        await using var estherContext = CreateContext();
        await using var leahContext = CreateContext();
        var estherRepository = new CourseGroupRepository(estherContext);
        var leahRepository = new CourseGroupRepository(leahContext);
        await TakeLastSeatAsync(estherContext, EstherId);
        await TakeLastSeatAsync(leahContext, LeahId);

        // Act + Assert: ה-Repository ממיר את החריגה של EF לחריגה שלנו,
        // וכך שכבת Application מטפלת בהתנגשות בלי להכיר את EF
        await estherRepository.SaveChangesAsync(CancellationToken.None);
        await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => leahRepository.SaveChangesAsync(CancellationToken.None));

        await AssertOnlyEstherGotTheSeatAsync();
    }

    private AppDbContext CreateContext() => new(_options);

    /// <summary>
    /// מה שהרישום עושה בתוך בקשה אחת: קורא את הקבוצה, מגדיל את מספר הנרשמות ומוסיף שורת רישום.
    /// עדיין בלי לשמור – כך שתי הבקשות קוראות לפני שמישהי מהן שמרה.
    /// </summary>
    private static async Task<CourseGroup> TakeLastSeatAsync(AppDbContext context, int memberId)
    {
        var group = await context.CourseGroups.SingleAsync(g => g.Id == DemoGroupId);
        Assert.True(group.EnrolledCount < group.Capacity); // שתיהן רואות מקום פנוי

        group.EnrolledCount++;
        context.Enrollments.Add(new Enrollment
        {
            UserId = memberId,
            CourseGroupId = group.Id,
            EnrolledAt = DateTime.UtcNow,
            Status = EnrollmentStatus.Active
        });

        return group;
    }

    /// <summary>
    /// בדיקה דרך DbContext שלישי ונקי, מול מה שנשמר בפועל בבסיס הנתונים.
    /// </summary>
    private async Task AssertOnlyEstherGotTheSeatAsync()
    {
        await using var context = CreateContext();

        // מספר הנרשמות עלה באחד בלבד: מ-1 ל-2, ולא ל-3
        var group = await context.CourseGroups.AsNoTracking().SingleAsync(g => g.Id == DemoGroupId);
        Assert.Equal(2, group.EnrolledCount);

        // רשומות בקבוצה רק חנה (מה-Seed) ואסתר. גם שורת הרישום של לאה לא נשמרה,
        // כי היא הייתה באותה שמירה (באותה טרנזקציה) שנכשלה
        var members = await context.Enrollments.AsNoTracking()
            .Where(e => e.CourseGroupId == DemoGroupId && e.Status == EnrollmentStatus.Active)
            .Select(e => e.UserId)
            .OrderBy(id => id)
            .ToListAsync();
        Assert.Equal(new[] { ChanaId, EstherId }, members);
    }

    public void Dispose() => _connection.Dispose();
}
