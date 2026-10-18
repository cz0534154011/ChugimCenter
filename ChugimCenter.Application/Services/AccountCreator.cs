using AutoMapper;
using ChugimCenter.Application.Common;
using ChugimCenter.Application.Common.Exceptions;
using ChugimCenter.Application.DTOs.Auth;
using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Application.Interfaces.Security;
using ChugimCenter.Domain.Entities;
using ChugimCenter.Domain.Enums;

namespace ChugimCenter.Application.Services;

/// <summary>
/// יצירת חשבון חדש: בדיקת אימייל קיים, הצפנת הסיסמה ושמירה.
/// משותף להרשמה עצמית של משתתפת וליצירת מדריכה על ידי המנהלת, כדי לא לכפול את הקוד.
/// </summary>
public class AccountCreator(IUserRepository users, IPasswordHasher passwordHasher, IMapper mapper)
{
    private const string EmailTakenMessage = "כתובת האימייל כבר רשומה במערכת";

    public async Task<Result<User>> CreateAsync(AccountRequestDto dto, UserRole role, CancellationToken ct)
    {
        if (await users.EmailExistsAsync(EmailNormalizer.Normalize(dto.Email), ct))
            return Result<User>.Failure(ErrorType.Conflict, EmailTakenMessage);

        // AutoMapper בוחר את המיפוי לפי הסוג בפועל (הרשמה או מדריכה), והאימייל נשמר בצורה אחידה
        var user = mapper.Map<User>(dto);
        user.Role = role;
        user.PasswordHash = passwordHasher.Hash(dto.Password);

        await users.AddAsync(user, ct);
        try
        {
            await users.SaveChangesAsync(ct);
        }
        catch (DuplicateEntryException)
        {
            // שתי הרשמות עם אותו אימייל באותו רגע: האינדקס הייחודי עצר את השנייה
            return Result<User>.Failure(ErrorType.Conflict, EmailTakenMessage);
        }

        return Result<User>.Success(user);
    }
}
