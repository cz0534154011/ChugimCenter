using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.Users;
using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Domain.Entities;
using ChugimCenter.Infrastructure.Extensions;
using ChugimCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChugimCenter.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
        => Set.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct)
        => Set.AnyAsync(u => u.Email == email, ct);

    public Task<PagedResult<User>> GetPagedAsync(UserQueryDto query, CancellationToken ct)
        => Set.AsNoTracking()
              .Where(u => query.Role == null || u.Role == query.Role)
              .OrderBy(u => u.FullName)
              .ToPagedResultAsync(query.Page, query.PageSize, ct);
}
