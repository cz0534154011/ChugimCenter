using System.ComponentModel.DataAnnotations;
using ChugimCenter.Application.DTOs.Auth;

namespace ChugimCenter.Application.DTOs.Users;

/// <summary>יצירת מדריכה על ידי המנהלת, כולל סיסמה ראשונית.</summary>
public class CreateInstructorDto : AccountRequestDto
{
    [Required(ErrorMessage = "הטלפון הוא שדה חובה")]
    [RegularExpression(@"^0\d{1,2}-?\d{7}$", ErrorMessage = "מספר הטלפון אינו תקין")]
    public string Phone { get; set; } = string.Empty;
}
