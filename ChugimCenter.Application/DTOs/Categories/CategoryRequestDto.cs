using System.ComponentModel.DataAnnotations;

namespace ChugimCenter.Application.DTOs.Categories;

/// <summary>יצירה או עדכון של תחום.</summary>
public class CategoryRequestDto
{
    [Required(ErrorMessage = "שם התחום הוא שדה חובה")]
    [MaxLength(50, ErrorMessage = "שם התחום יכול להכיל עד 50 תווים")]
    public string Name { get; set; } = string.Empty;
}
