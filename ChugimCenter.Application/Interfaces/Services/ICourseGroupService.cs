using ChugimCenter.Application.Common;
using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.CourseGroups;
using ChugimCenter.Application.DTOs.Users;

namespace ChugimCenter.Application.Interfaces.Services;

public interface ICourseGroupService
{
    /// <summary>user הוא null כשהגולשת לא מחוברת. מנהלת רואה גם קבוצות לא פעילות.</summary>
    Task<PagedResult<CourseGroupDto>> GetPagedAsync(CourseGroupQueryDto query, CurrentUser? user, CancellationToken ct);
    Task<Result<CourseGroupDto>> GetByIdAsync(int id, CurrentUser? user, CancellationToken ct);

    Task<Result<CourseGroupDto>> CreateAsync(CourseGroupRequestDto dto, CancellationToken ct);
    Task<Result<CourseGroupDto>> UpdateAsync(int id, CourseGroupRequestDto dto, CancellationToken ct);
    Task<Result> SetActiveAsync(int id, bool isActive, CancellationToken ct);
    Task<Result> DeleteAsync(int id, CancellationToken ct);

    Task<List<CourseGroupDto>> GetInstructorGroupsAsync(int instructorId, CancellationToken ct);
    Task<Result<List<StudentDto>>> GetStudentsAsync(int groupId, CurrentUser user, CancellationToken ct);
}
