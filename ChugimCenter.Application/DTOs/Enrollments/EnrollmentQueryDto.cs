using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Domain.Enums;

namespace ChugimCenter.Application.DTOs.Enrollments;

/// <summary>סינון רשימת הרישומים. משמש גם את "החוגים שלי" וגם את המנהלת.</summary>
public class EnrollmentQueryDto : PagingQuery
{
    public int? CourseGroupId { get; set; }
    public EnrollmentStatus? Status { get; set; }
    public bool? IsPaid { get; set; }
}
