using ChugimCenter.Domain.Entities;

namespace ChugimCenter.Application.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<List<Category>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct);
    Task<bool> HasGroupsAsync(int categoryId, CancellationToken ct);
}
