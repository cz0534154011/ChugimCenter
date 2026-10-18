using ChugimCenter.Application.Interfaces.Security;

namespace ChugimCenter.Infrastructure.Security;

/// <summary>המימוש של הצפנת הסיסמאות, עם BCrypt.</summary>
public class BcryptPasswordHasher : IPasswordHasher
{
    // מספר הסבבים הוא 2 בחזקת 11. זהה לערך שבו נוצרו ה-hash-ים ב-SeedData
    private const int WorkFactor = 11;

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    // מספר הסבבים והמלח (salt) שמורים בתוך ה-hash עצמו, ולכן Verify לא צריך אותם בנפרד
    public bool Verify(string password, string passwordHash)
        => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
