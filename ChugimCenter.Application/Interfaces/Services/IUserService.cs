using ChugimCenter.Application.Common;
using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.Users;

namespace ChugimCenter.Application.Interfaces.Services;

public interface IUserService
{
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<UserDto>> CreateInstructorAsync(CreateInstructorDto dto, CancellationToken ct);
    Task<PagedResult<UserDto>> GetPagedAsync(UserQueryDto query, CancellationToken ct);
}
