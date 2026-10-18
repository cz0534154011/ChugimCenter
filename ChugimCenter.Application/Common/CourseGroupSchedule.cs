using ChugimCenter.Domain.Entities;

namespace ChugimCenter.Application.Common;

/// <summary>
/// חישובי זמנים של קבוצת חוג, במקום אחד:
/// תאריך הסיום, ובדיקה אם שתי קבוצות מתנגשות בזמן.
/// משמש גם את בדיקת החפיפה אצל המדריכה וגם אצל המשתתפת.
/// </summary>
public static class CourseGroupSchedule
{
    private const int DaysInWeek = 7;

    /// <summary>תאריך המפגש האחרון: תאריך ההתחלה, ועוד שבוע לכל מפגש נוסף.</summary>
    public static DateOnly GetEndDate(DateOnly startDate, int sessionsCount)
        => startDate.AddDays((sessionsCount - 1) * DaysInWeek);

    public static DateOnly GetEndDate(CourseGroup group)
        => GetEndDate(group.StartDate, group.SessionsCount);

    /// <summary>
    /// שתי קבוצות מתנגשות כשהן באותו יום בשבוע, השעות חופפות והתקופות חופפות.
    /// היום נלקח מתאריך ההתחלה ולא מהשדה DayOfWeek, כי בקבוצה חדשה הוא עוד לא מולא (הוא מתמלא בשמירה).
    /// </summary>
    public static bool Overlaps(CourseGroup a, CourseGroup b)
    {
        if (a.StartDate.DayOfWeek != b.StartDate.DayOfWeek)
            return false;

        var periodsOverlap = a.StartDate <= GetEndDate(b) && b.StartDate <= GetEndDate(a);

        var aStart = a.StartTime.ToTimeSpan();
        var bStart = b.StartTime.ToTimeSpan();
        var aEnd = aStart + TimeSpan.FromMinutes(a.DurationMinutes);
        var bEnd = bStart + TimeSpan.FromMinutes(b.DurationMinutes);

        // חפיפה אמיתית בלבד: קבוצה שמסתיימת ב-17:00 לא מתנגשת בקבוצה שמתחילה ב-17:00
        var hoursOverlap = aStart < bEnd && bStart < aEnd;

        return periodsOverlap && hoursOverlap;
    }
}
