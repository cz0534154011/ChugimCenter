using ChugimCenter.Application.Common;
using ChugimCenter.Application.DTOs.Categories;

namespace ChugimCenter.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(bool includeInactive, CancellationToken ct);
    Task<Result<CategoryDto>> GetByIdAsync(int id, bool includeInactive, CancellationToken ct);
    Task<Result<CategoryDto>> CreateAsync(CategoryRequestDto dto, CancellationToken ct);
    Task<Result<CategoryDto>> UpdateAsync(int id, CategoryRequestDto dto, CancellationToken ct);
    Task<Result> SetActiveAsync(int id, bool isActive, CancellationToken ct);
    Task<Result> DeleteAsync(int id, CancellationToken ct);
}
