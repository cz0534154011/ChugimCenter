namespace ChugimCenter.Application.Common;

/// <summary>
/// צורה אחידה לאימייל: בלי רווחים ובאותיות קטנות.
/// כך Sara@Example.com ו-sara@example.com נחשבים לאותה כתובת.
/// משמש בהרשמה, בהתחברות וביצירת מדריכה – כתוב פעם אחת כאן.
/// </summary>
public static class EmailNormalizer
{
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
