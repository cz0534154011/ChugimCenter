namespace ChugimCenter.Application.Interfaces.Security;

/// <summary>
/// הצפנה ובדיקה של סיסמאות.
/// Application מגדירה רק "מה צריך"; המימוש (BCrypt) יושב ב-Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>הופך סיסמה למחרוזת מוצפנת, שממנה אי אפשר לשחזר את הסיסמה.</summary>
    string Hash(string password);

    /// <summary>בודק אם הסיסמה שהוקלדה מתאימה למחרוזת המוצפנת השמורה.</summary>
    bool Verify(string password, string passwordHash);
}
