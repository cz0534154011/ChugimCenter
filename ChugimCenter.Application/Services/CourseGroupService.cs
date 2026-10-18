using AutoMapper;
using ChugimCenter.Application.Common;
using ChugimCenter.Application.Common.Exceptions;
using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.CourseGroups;
using ChugimCenter.Application.DTOs.Users;
using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Application.Interfaces.Services;
using ChugimCenter.Application.Mapping;
using ChugimCenter.Domain.Entities;
using ChugimCenter.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ChugimCenter.Application.Services;

public class CourseGroupService(
    ICourseGroupRepository groups,
    ICategoryRepository categories,
    IUserRepository users,
    IEnrollmentRepository enrollments,
    IMapper mapper,
    TimeProvider timeProvider,
    ILogger<CourseGroupService> logger) : ICourseGroupService
{
    /// <summary>מנהלת רואה הכל. כל השאר (גם גולשת לא מחוברת) רואים רק קבוצות פעילות בתחומים פעילים.</summary>
    public async Task<PagedResult<CourseGroupDto>> GetPagedAsync(
        CourseGroupQueryDto query, CurrentUser? user, CancellationToken ct)
    {
        var page = await groups.GetPagedAsync(query, IsAdmin(user), Today(), ct);
        return mapper.MapPaged<CourseGroup, CourseGroupDto>(page);
    }

    public async Task<Result<CourseGroupDto>> GetByIdAsync(int id, CurrentUser? user, CancellationToken ct)
    {
        var group = await groups.GetDetailsAsync(id, ct);
        var hidden = group is not null && !IsAdmin(user) && (!group.IsActive || !group.Category.IsActive);

        if (group is null || hidden)
            return NotFound<CourseGroupDto>(id);

        return Result<CourseGroupDto>.Success(mapper.Map<CourseGroupDto>(group));
    }

    public async Task<Result<CourseGroupDto>> CreateAsync(CourseGroupRequestDto dto, CancellationToken ct)
    {
        var group = mapper.Map<CourseGroup>(dto);

        if (group.StartDate < Today())
            return Result<CourseGroupDto>.Failure(ErrorType.Validation, "תאריך ההתחלה לא יכול להיות בעבר");

        var invalid = await ValidateAsync(group, excludeGroupId: null, checkCategory: true, ct);
        if (invalid is not null)
            return invalid.ToFailure<CourseGroupDto>();

        await groups.AddAsync(group, ct);
        await groups.SaveChangesAsync(ct);

        // טעינה מחדש עם התחום והמדריכה, כדי שהתשובה תכלול את השמות
        return await GetDetailsDtoAsync(group.Id, ct);
    }

    public async Task<Result<CourseGroupDto>> UpdateAsync(int id, CourseGroupRequestDto dto, CancellationToken ct)
    {
        var group = await groups.GetByIdAsync(id, ct);
        if (group is null)
            return NotFound<CourseGroupDto>(id);

        // בדיקת התחום רק כשהוא מוחלף, כדי שאפשר יהיה לעדכן קבוצה קיימת גם בתחום שהושבת
        var categoryChanged = dto.CategoryId != group.CategoryId;

        // מעתיק רק את שדות הבקשה. מספר הנרשמות, הסטטוס והגרסה לא נדרסים
        mapper.Map(dto, group);

        if (group.Capacity < group.EnrolledCount)
            return Result<CourseGroupDto>.Failure(ErrorType.Conflict,
                $"מספר המקומות לא יכול להיות קטן ממספר הנרשמות ({group.EnrolledCount})");

        var invalid = await ValidateAsync(group, excludeGroupId: id, checkCategory: categoryChanged, ct);
        if (invalid is not null)
            return invalid.ToFailure<CourseGroupDto>();

        var saved = await SaveWithConcurrencyAsync(id, ct);
        if (!saved.IsSuccess)
            return saved.ToFailure<CourseGroupDto>();

        return await GetDetailsDtoAsync(id, ct);
    }

    public async Task<Result> SetActiveAsync(int id, bool isActive, CancellationToken ct)
    {
        var group = await groups.GetByIdAsync(id, ct);
        if (group is null)
            return NotFound<CourseGroupDto>(id);

        group.IsActive = isActive;
        return await SaveWithConcurrencyAsync(id, ct);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var group = await groups.GetByIdAsync(id, ct);
        if (group is null)
            return NotFound<CourseGroupDto>(id);

        if (await groups.HasEnrollmentsAsync(id, ct))
            return Result.Failure(ErrorType.Conflict,
                "לא ניתן למחוק קבוצה שהיו בה רישומים (גם מבוטלים). אפשר להשבית אותה במקום זאת.");

        groups.Remove(group);
        await groups.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<List<CourseGroupDto>> GetInstructorGroupsAsync(int instructorId, CancellationToken ct)
        => mapper.Map<List<CourseGroupDto>>(await groups.GetByInstructorAsync(instructorId, ct));

    /// <summary>רשימת התלמידות: רק המדריכה של הקבוצה או המנהלת. מוצגים רק רישומים פעילים.</summary>
    public async Task<Result<List<StudentDto>>> GetStudentsAsync(int groupId, CurrentUser user, CancellationToken ct)
    {
        var group = await groups.GetByIdAsync(groupId, ct);
        if (group is null)
            return NotFound<List<StudentDto>>(groupId);

        if (!user.IsAdmin && group.InstructorId != user.Id)
            return Result<List<StudentDto>>.Failure(ErrorType.Forbidden, "אינך המדריכה של קבוצה זו");

        var active = await enrollments.GetActiveByGroupAsync(groupId, ct);
        return Result<List<StudentDto>>.Success(mapper.Map<List<StudentDto>>(active));
    }

    /// <summary>
    /// הבדיקות המשותפות ליצירה ולעדכון. מחזיר null כשהכל תקין, או את הכישלון הראשון.
    /// </summary>
    private async Task<Result?> ValidateAsync(
        CourseGroup group, int? excludeGroupId, bool checkCategory, CancellationToken ct)
    {
        if (group.MinAge > group.MaxAge)
            return Result.Failure(ErrorType.Validation, "הגיל המינימלי לא יכול להיות גדול מהגיל המקסימלי");

        if (checkCategory)
        {
            var category = await categories.GetByIdAsync(group.CategoryId, ct);
            if (category is null || !category.IsActive)
                return Result.Failure(ErrorType.Validation, "התחום שנבחר לא קיים או לא פעיל");
        }

        var instructor = await users.GetByIdAsync(group.InstructorId, ct);
        if (instructor is null || instructor.Role != UserRole.Instructor)
            return Result.Failure(ErrorType.Validation, "המדריכה שנבחרה לא קיימת");

        // חפיפה בזמן אצל המדריכה. אין בדיקת חדרים
        var sameDay = await groups.GetInstructorGroupsOnDayAsync(
            group.InstructorId, group.StartDate.DayOfWeek, excludeGroupId, ct);
        var clash = sameDay.FirstOrDefault(other => CourseGroupSchedule.Overlaps(group, other));
        if (clash is not null)
            return Result.Failure(ErrorType.Conflict, $"למדריכה יש קבוצה אחרת באותו זמן: {clash.Name}");

        return null;
    }

    /// <summary>
    /// שמירה של קבוצה קיימת. אם בקשה אחרת (למשל רישום) שינתה את הקבוצה בינתיים,
    /// הגרסה כבר לא תואמת, ומוחזר 409 במקום לדרוס את השינוי שלה.
    /// </summary>
    private async Task<Result> SaveWithConcurrencyAsync(int groupId, CancellationToken ct)
    {
        try
        {
            await groups.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (ConcurrencyConflictException)
        {
            logger.LogWarning("Concurrency conflict while updating course group {GroupId}", groupId);
            return Result.Failure(ErrorType.Conflict, "הקבוצה עודכנה בינתיים על ידי פעולה אחרת. נסי שוב.");
        }
    }

    private async Task<Result<CourseGroupDto>> GetDetailsDtoAsync(int id, CancellationToken ct)
    {
        var group = await groups.GetDetailsAsync(id, ct);
        return group is null
            ? NotFound<CourseGroupDto>(id)
            : Result<CourseGroupDto>.Success(mapper.Map<CourseGroupDto>(group));
    }

    private DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

    private static bool IsAdmin(CurrentUser? user) => user?.IsAdmin == true;

    private static Result<T> NotFound<T>(int id)
        => Result<T>.Failure(ErrorType.NotFound, $"קבוצה מספר {id} לא נמצאה");
}
