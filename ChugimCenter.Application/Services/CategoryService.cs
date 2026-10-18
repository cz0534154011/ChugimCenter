using AutoMapper;
using ChugimCenter.Application.Common;
using ChugimCenter.Application.Common.Exceptions;
using ChugimCenter.Application.DTOs.Categories;
using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Application.Interfaces.Services;
using ChugimCenter.Domain.Entities;

namespace ChugimCenter.Application.Services;

public class CategoryService(ICategoryRepository categories, IMapper mapper) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
        => mapper.Map<List<CategoryDto>>(await categories.GetAllAsync(includeInactive, ct));

    /// <summary>includeInactive הוא true רק למנהלת. לאחרות, תחום לא פעיל "לא קיים" (404).</summary>
    public async Task<Result<CategoryDto>> GetByIdAsync(int id, bool includeInactive, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(id, ct);
        if (category is null || (!category.IsActive && !includeInactive))
            return NotFound<CategoryDto>(id);

        return Result<CategoryDto>.Success(mapper.Map<CategoryDto>(category));
    }

    public async Task<Result<CategoryDto>> CreateAsync(CategoryRequestDto dto, CancellationToken ct)
    {
        if (await categories.NameExistsAsync(dto.Name, null, ct))
            return NameTaken(dto.Name);

        var category = mapper.Map<Category>(dto);
        await categories.AddAsync(category, ct);
        return await SaveAndMapAsync(category, ct);
    }

    public async Task<Result<CategoryDto>> UpdateAsync(int id, CategoryRequestDto dto, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(id, ct);
        if (category is null)
            return NotFound<CategoryDto>(id);

        if (await categories.NameExistsAsync(dto.Name, id, ct))
            return NameTaken(dto.Name);

        mapper.Map(dto, category);
        return await SaveAndMapAsync(category, ct);
    }

    public async Task<Result> SetActiveAsync(int id, bool isActive, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(id, ct);
        if (category is null)
            return NotFound<CategoryDto>(id);

        category.IsActive = isActive;
        await categories.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(id, ct);
        if (category is null)
            return NotFound<CategoryDto>(id);

        if (await categories.HasGroupsAsync(id, ct))
            return Result.Failure(ErrorType.Conflict,
                "לא ניתן למחוק תחום שיש בו קבוצות. אפשר להשבית אותו במקום זאת.");

        categories.Remove(category);
        await categories.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>שמירה משותפת ליצירה ולעדכון, עם רשת הביטחון של האינדקס הייחודי על השם.</summary>
    private async Task<Result<CategoryDto>> SaveAndMapAsync(Category category, CancellationToken ct)
    {
        try
        {
            await categories.SaveChangesAsync(ct);
        }
        catch (DuplicateEntryException)
        {
            return NameTaken(category.Name);
        }

        return Result<CategoryDto>.Success(mapper.Map<CategoryDto>(category));
    }

    private static Result<CategoryDto> NameTaken(string name)
        => Result<CategoryDto>.Failure(ErrorType.Conflict, $"כבר קיים תחום בשם \"{name}\"");

    private static Result<T> NotFound<T>(int id)
        => Result<T>.Failure(ErrorType.NotFound, $"תחום מספר {id} לא נמצא");
}
