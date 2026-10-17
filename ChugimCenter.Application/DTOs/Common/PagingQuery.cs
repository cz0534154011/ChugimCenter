using System.ComponentModel.DataAnnotations;

namespace ChugimCenter.Application.DTOs.Common;

/// <summary>פרמטרים משותפים לכל הרשימות עם pagination.</summary>
public class PagingQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "מספר העמוד חייב להיות 1 ומעלה")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "גודל העמוד חייב להיות בין 1 ל-50")]
    public int PageSize { get; set; } = 10;
}
