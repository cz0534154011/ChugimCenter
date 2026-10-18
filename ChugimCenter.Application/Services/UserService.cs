using AutoMapper;
using ChugimCenter.Application.Common;
using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.Users;
using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Application.Interfaces.Services;
using ChugimCenter.Application.Mapping;
using ChugimCenter.Domain.Entities;
using ChugimCenter.Domain.Enums;

namespace ChugimCenter.Application.Services;

public class UserService(IUserRepository users, AccountCreator accountCreator, IMapper mapper) : IUserService
{
    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(id, ct);
        if (user is null)
            return Result<UserDto>.Failure(ErrorType.NotFound, $"משתמשת מספר {id} לא נמצאה");

        return Result<UserDto>.Success(mapper.Map<UserDto>(user));
    }

    /// <summary>רק המנהלת יוצרת מדריכה (נבדק ב-Controller). התפקיד נקבע כאן, ולא מהבקשה.</summary>
    public async Task<Result<UserDto>> CreateInstructorAsync(CreateInstructorDto dto, CancellationToken ct)
    {
        var created = await accountCreator.CreateAsync(dto, UserRole.Instructor, ct);
        if (!created.IsSuccess)
            return created.ToFailure<UserDto>();

        return Result<UserDto>.Success(mapper.Map<UserDto>(created.Value!));
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(UserQueryDto query, CancellationToken ct)
        => mapper.MapPaged<User, UserDto>(await users.GetPagedAsync(query, ct));
}
