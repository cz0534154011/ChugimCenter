using ChugimCenter.Domain.Enums;

namespace ChugimCenter.API.Authorization;

/// <summary>
/// שמות התפקידים לשימוש ב-[Authorize(Roles = ...)].
/// נגזרים מה-enum, כדי ששינוי שם בתפקיד ייתפס כבר בקומפילציה ולא יישבר בשקט.
/// </summary>
public static class Roles
{
    public const string Admin = nameof(UserRole.Admin);
    public const string Instructor = nameof(UserRole.Instructor);
    public const string Member = nameof(UserRole.Member);
    public const string AdminOrInstructor = Admin + "," + Instructor;
}
