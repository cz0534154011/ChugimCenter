using AutoMapper;
using ChugimCenter.Application.Common;
using ChugimCenter.Application.Common.Exceptions;
using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.Enrollments;
using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Application.Interfaces.Services;
using ChugimCenter.Application.Mapping;
using ChugimCenter.Domain.Entities;
using ChugimCenter.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ChugimCenter.Application.Services;

/// <summary>
/// הרישום לחוג – הלב של הפרויקט.
/// המשאב המוגבל הוא מקום בקבוצה. כל רישום וכל ביטול מעדכנים את שורת הקבוצה (EnrolledCount),
/// ולכן שתי בקשות מתחרות "נפגשות" באותה שורה, וה-concurrency token (Version) מזהה את ההתנגשות.
/// </summary>
public class EnrollmentService(
    IEnrollmentRepository enrollments,
    ICourseGroupRepository groups,
    ICategoryRepository categories,
    IUserRepository users,
    IMapper mapper,
    TimeProvider timeProvider,
    ILogger<EnrollmentService> logger) : IEnrollmentService
{
    private const string SeatTakenMessage = "המקום נתפס על ידי משתמשת אחרת. נסי שוב.";
    private const string GroupChangedMessage = "הקבוצה עודכנה בינתיים על ידי פעולה אחרת. נסי שוב.";
    private const string AlreadyEnrolledMessage = "את כבר רשומה לקבוצה זו";

    public async Task<Result<EnrollmentDto>> EnrollAsync(CreateEnrollmentDto dto, CurrentUser user, CancellationToken ct)
    {
        if (user.Role != UserRole.Member)
            return Result<EnrollmentDto>.Failure(ErrorType.Forbidden, "רק משתתפת יכולה להירשם לחוג");

        // שליפה במצב מעקב: הקבוצה תעודכן, והגרסה שנקראה עכשיו תיכנס לתנאי ה-WHERE בשמירה
        var group = await groups.GetByIdAsync(dto.CourseGroupId, ct);
        var category = group is null ? null : await categories.GetByIdAsync(group.CategoryId, ct);
        if (group is null || !group.IsActive || category is null || !category.IsActive)
            return Result<EnrollmentDto>.Failure(ErrorType.NotFound, $"קבוצה מספר {dto.CourseGroupId} לא נמצאה");

        if (CourseGroupSchedule.GetEndDate(group) < Today())
            return Result<EnrollmentDto>.Failure(ErrorType.Validation, "החוג כבר הסתיים");

        var member = await users.GetByIdAsync(user.Id, ct);
        if (member?.BirthDate is null)
            return Result<EnrollmentDto>.Failure(ErrorType.Validation, "חסר תאריך לידה בפרטי המשתתפת");

        // הגיל נבדק לפי תאריך תחילת החוג, ולא לפי היום
        var age = AgeAt(member.BirthDate.Value, group.StartDate);
        if (age < group.MinAge || age > group.MaxAge)
            return Result<EnrollmentDto>.Failure(ErrorType.Validation,
                $"קבוצה זו מיועדת לגילאי {group.MinAge}–{group.MaxAge}");

        if (await enrollments.HasActiveEnrollmentAsync(user.Id, group.Id, ct))
            return Result<EnrollmentDto>.Failure(ErrorType.Conflict, AlreadyEnrolledMessage);

        // חפיפה בזמן עם חוג אחר של המשתתפת – אותה בדיקה כמו אצל המדריכה
        var sameDay = await enrollments.GetActiveGroupsOfUserOnDayAsync(user.Id, group.StartDate.DayOfWeek, ct);
        var clash = sameDay.FirstOrDefault(other => CourseGroupSchedule.Overlaps(group, other));
        if (clash is not null)
            return Result<EnrollmentDto>.Failure(ErrorType.Conflict, $"את רשומה לחוג אחר באותו זמן: {clash.Name}");

        if (group.EnrolledCount >= group.Capacity)
            return Result<EnrollmentDto>.Failure(ErrorType.Conflict, "אין מקומות פנויים בקבוצה");

        var enrollment = new Enrollment
        {
            UserId = user.Id,
            CourseGroupId = group.Id,
            EnrolledAt = Now(),
            Status = EnrollmentStatus.Active
        };

        // הרישום החדש והגדלת מספר הנרשמות נשמרים יחד, בשמירה אחת (טרנזקציה אחת)
        group.EnrolledCount++;
        await enrollments.AddAsync(enrollment, ct);

        var saved = await SaveAsync(group.Id, SeatTakenMessage, ct);
        if (!saved.IsSuccess)
            return saved.ToFailure<EnrollmentDto>();

        return await GetDtoAsync(enrollment.Id, ct);
    }

    /// <summary>רישום בודד: רק בעלת הרישום או המנהלת. משתתפת אחרת מקבלת 403.</summary>
    public async Task<Result<EnrollmentDto>> GetByIdAsync(int enrollmentId, CurrentUser user, CancellationToken ct)
    {
        var enrollment = await enrollments.GetWithGroupAsync(enrollmentId, ct);
        if (enrollment is null)
            return NotFound<EnrollmentDto>(enrollmentId);

        if (!user.IsAdmin && enrollment.UserId != user.Id)
            return Result<EnrollmentDto>.Failure(ErrorType.Forbidden, "אין לך הרשאה לצפות ברישום זה");

        return Result<EnrollmentDto>.Success(mapper.Map<EnrollmentDto>(enrollment));
    }

    /// <summary>
    /// ביטול: השורה לא נמחקת, רק הסטטוס משתנה, ומספר הנרשמות קטן – באותה שמירה ועם אותה הגנת גרסה.
    /// רישום מחדש אחר כך ייצור שורה חדשה, והישנה תישאר בהיסטוריה.
    /// </summary>
    public async Task<Result> CancelAsync(int enrollmentId, CurrentUser user, CancellationToken ct)
    {
        var enrollment = await enrollments.GetWithGroupAsync(enrollmentId, ct);
        if (enrollment is null)
            return NotFound<EnrollmentDto>(enrollmentId);

        if (!user.IsAdmin && enrollment.UserId != user.Id)
            return Result.Failure(ErrorType.Forbidden, "אין לך הרשאה לבטל רישום זה");

        if (enrollment.Status == EnrollmentStatus.Cancelled)
            return Result.Failure(ErrorType.Conflict, "הרישום כבר בוטל");

        if (enrollment.IsPaid && !user.IsAdmin)
            return Result.Failure(ErrorType.Conflict, "הרישום כבר שולם, ולכן אפשר לבטל אותו רק דרך המזכירות");

        enrollment.Status = EnrollmentStatus.Cancelled;
        enrollment.CancelledAt = Now();
        enrollment.CancelledById = user.Id;
        enrollment.CourseGroup.EnrolledCount--;

        return await SaveAsync(enrollment.CourseGroupId, GroupChangedMessage, ct);
    }

    /// <summary>"החוגים שלי": כשלא נשלח סטטוס, מוצגים רק רישומים פעילים.</summary>
    public async Task<PagedResult<EnrollmentDto>> GetMineAsync(EnrollmentQueryDto query, int userId, CancellationToken ct)
    {
        query.Status ??= EnrollmentStatus.Active;
        return mapper.MapPaged<Enrollment, EnrollmentDto>(await enrollments.GetPagedAsync(query, userId, ct));
    }

    /// <summary>כל הרישומים, למנהלת. בלי ברירת מחדל לסטטוס: המנהלת רואה הכל, אלא אם סיננה.</summary>
    public async Task<PagedResult<EnrollmentDto>> GetPagedAsync(EnrollmentQueryDto query, CancellationToken ct)
        => mapper.MapPaged<Enrollment, EnrollmentDto>(await enrollments.GetPagedAsync(query, null, ct));

    /// <summary>
    /// סימון תשלום על ידי המנהלת (אין סליקה). אי אפשר לסמן תשלום חדש על רישום מבוטל,
    /// אבל מותר לשנות סימון קיים (למשל לבטל "שולם" אחרי החזר כספי).
    /// </summary>
    public async Task<Result<EnrollmentDto>> SetPaymentAsync(int enrollmentId, bool isPaid, int adminId, CancellationToken ct)
    {
        var enrollment = await enrollments.GetWithGroupAsync(enrollmentId, ct);
        if (enrollment is null)
            return NotFound<EnrollmentDto>(enrollmentId);

        var isNewPayment = isPaid && !enrollment.IsPaid;
        if (isNewPayment && enrollment.Status == EnrollmentStatus.Cancelled)
            return Result<EnrollmentDto>.Failure(ErrorType.Conflict, "לא ניתן לסמן תשלום חדש על רישום מבוטל");

        if (enrollment.IsPaid != isPaid)
        {
            enrollment.IsPaid = isPaid;
            enrollment.PaidAt = isPaid ? Now() : null;
            enrollment.PaidById = isPaid ? adminId : null;

            // רק שורת הרישום משתנה (לא הקבוצה), ולכן אין כאן תחרות על מקום
            await enrollments.SaveChangesAsync(ct);

            logger.LogInformation("Enrollment {EnrollmentId} marked as {PaymentStatus} by admin {AdminId}",
                enrollmentId, isPaid ? "paid" : "unpaid", adminId);
        }

        return Result<EnrollmentDto>.Success(mapper.Map<EnrollmentDto>(enrollment));
    }

    /// <summary>
    /// השמירה המשותפת לרישום ולביטול. ה-Repositories חולקים את אותו DbContext,
    /// ולכן שמירה אחת כוללת גם את שורת הרישום וגם את שורת הקבוצה.
    /// </summary>
    private async Task<Result> SaveAsync(int groupId, string conflictMessage, CancellationToken ct)
    {
        try
        {
            await enrollments.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (ConcurrencyConflictException)
        {
            // בקשה אחרת שינתה את הקבוצה בין הקריאה לשמירה: הגרסה לא תאמה, ואף שורה לא עודכנה
            logger.LogWarning("Concurrency conflict on course group {GroupId}", groupId);
            return Result.Failure(ErrorType.Conflict, conflictMessage);
        }
        catch (DuplicateEntryException)
        {
            // רשת הביטחון: האינדקס הייחודי על רישום פעיל עצר רישום כפול לאותה קבוצה
            logger.LogWarning("Duplicate active enrollment blocked for course group {GroupId}", groupId);
            return Result.Failure(ErrorType.Conflict, AlreadyEnrolledMessage);
        }
    }

    private async Task<Result<EnrollmentDto>> GetDtoAsync(int enrollmentId, CancellationToken ct)
    {
        var enrollment = await enrollments.GetWithGroupAsync(enrollmentId, ct);
        return enrollment is null
            ? NotFound<EnrollmentDto>(enrollmentId)
            : Result<EnrollmentDto>.Success(mapper.Map<EnrollmentDto>(enrollment));
    }

    /// <summary>גיל בתאריך מסוים: הפרש השנים, פחות אחת אם יום ההולדת עוד לא הגיע באותה שנה.</summary>
    private static int AgeAt(DateOnly birthDate, DateOnly date)
    {
        var age = date.Year - birthDate.Year;
        if (birthDate > date.AddYears(-age))
            age--;
        return age;
    }

    private DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;

    private DateOnly Today() => DateOnly.FromDateTime(Now());

    private static Result<T> NotFound<T>(int id)
        => Result<T>.Failure(ErrorType.NotFound, $"רישום מספר {id} לא נמצא");
}
