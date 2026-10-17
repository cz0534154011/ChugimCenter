using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.Users;
using ChugimCenter.Domain.Entities;

namespace ChugimCenter.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task<PagedResult<User>> GetPagedAsync(UserQueryDto query, CancellationToken ct);
}
