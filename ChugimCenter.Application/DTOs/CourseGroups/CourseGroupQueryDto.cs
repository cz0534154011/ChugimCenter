using System.ComponentModel.DataAnnotations;
using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.Enums;

namespace ChugimCenter.Application.DTOs.CourseGroups;

/// <summary>סינון, מיון ו-pagination לרשימת הקבוצות.</summary>
public class CourseGroupQueryDto : PagingQuery
{
    public int? CategoryId { get; set; }
    public int? InstructorId { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }

    [Range(3, 18, ErrorMessage = "הגיל חייב להיות בין 3 ל-18")]
    public int? Age { get; set; }

    public bool OnlyAvailable { get; set; }
    public bool IncludeEnded { get; set; }

    public CourseGroupSortBy SortBy { get; set; } = CourseGroupSortBy.StartDate;
    public bool Descending { get; set; }
}
