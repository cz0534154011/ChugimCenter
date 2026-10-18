using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Domain.Entities;
using ChugimCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChugimCenter.Infrastructure.Repositories;

public class CategoryRepository(AppDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public Task<List<Category>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => Set.AsNoTracking()
              .Where(c => includeInactive || c.IsActive)
              .OrderBy(c => c.Name)
              .ToListAsync(ct);

    public Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct)
        => Set.AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId), ct);

    public Task<bool> HasGroupsAsync(int categoryId, CancellationToken ct)
        => Context.CourseGroups.AnyAsync(g => g.CategoryId == categoryId, ct);
}
