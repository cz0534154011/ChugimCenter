using System.Security.Claims;
using ChugimCenter.Application.Common;
using ChugimCenter.Domain.Enums;

namespace ChugimCenter.API.Extensions;

/// <summary>
/// קריאת המשתמשת המחוברת מתוך ה-token החתום.
/// התפקיד נלקח מה-token ולא מהבקשה, ולכן אי אפשר לזייף אותו.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>למתודות עם [Authorize]: תמיד יש משתמשת מחוברת.</summary>
    public static CurrentUser ToCurrentUser(this ClaimsPrincipal principal)
    {
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? throw new InvalidOperationException("The token does not contain a user id.");
        var role = principal.FindFirstValue(ClaimTypes.Role)
                   ?? throw new InvalidOperationException("The token does not contain a role.");

        return new CurrentUser(int.Parse(id), Enum.Parse<UserRole>(role));
    }

    /// <summary>למתודות ציבוריות: null כשהגולשת לא מחוברת.</summary>
    public static CurrentUser? ToCurrentUserOrNull(this ClaimsPrincipal principal)
        => principal.Identity?.IsAuthenticated == true ? principal.ToCurrentUser() : null;
}
