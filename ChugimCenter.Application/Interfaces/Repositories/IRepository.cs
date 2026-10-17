namespace ChugimCenter.Application.Interfaces.Repositories;

/// <summary>פעולות בסיסיות שמשותפות לכל ה-Repositories.</summary>
public interface IRepository<T> where T : class
{
    /// <summary>שליפה לפי מזהה, במצב מעקב (לצורך עדכון).</summary>
    Task<T?> GetByIdAsync(int id, CancellationToken ct);

    Task AddAsync(T entity, CancellationToken ct);
    void Remove(T entity);
    Task SaveChangesAsync(CancellationToken ct);
}
