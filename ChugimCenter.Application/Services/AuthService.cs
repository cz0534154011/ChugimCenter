using AutoMapper;
using ChugimCenter.Application.Common;
using ChugimCenter.Application.DTOs.Auth;
using ChugimCenter.Application.DTOs.Users;
using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Application.Interfaces.Security;
using ChugimCenter.Application.Interfaces.Services;
using ChugimCenter.Domain.Entities;
using ChugimCenter.Domain.Enums;

namespace ChugimCenter.Application.Services;

public class AuthService(
    AccountCreator accountCreator,
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    IMapper mapper) : IAuthService
{
    // הודעה כללית בכוונה: לא מגלים אם האימייל קיים במערכת
    private const string InvalidCredentialsMessage = "האימייל או הסיסמה שגויים";

    /// <summary>הרשמה עצמית. התפקיד תמיד Member, ולא נלקח מהבקשה.</summary>
    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct)
    {
        var created = await accountCreator.CreateAsync(dto, UserRole.Member, ct);
        if (!created.IsSuccess)
            return created.ToFailure<AuthResponseDto>();

        return Result<AuthResponseDto>.Success(BuildResponse(created.Value!));
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(EmailNormalizer.Normalize(dto.Email), ct);

        if (user is null || !passwordHasher.Verify(dto.Password, user.PasswordHash))
            return Result<AuthResponseDto>.Failure(ErrorType.Unauthorized, InvalidCredentialsMessage);

        return Result<AuthResponseDto>.Success(BuildResponse(user));
    }

    private AuthResponseDto BuildResponse(User user)
    {
        var token = tokenGenerator.Generate(user);
        return new AuthResponseDto
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            User = mapper.Map<UserDto>(user)
        };
    }
}
