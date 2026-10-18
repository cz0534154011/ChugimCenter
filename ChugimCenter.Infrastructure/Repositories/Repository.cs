using ChugimCenter.Application.Common.Exceptions;
using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ChugimCenter.Infrastructure.Repositories;

/// <summary>הפעולות הבסיסיות, כתובות פעם אחת לכל ה-Repositories.</summary>
public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
{
    protected readonly AppDbContext Context = context;
    protected DbSet<T> Set => Context.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct)
        => await Set.FindAsync([id], ct);

    public async Task AddAsync(T entity, CancellationToken ct)
        => await Set.AddAsync(entity, ct);

    public void Remove(T entity)
        => Set.Remove(entity);

    /// <summary>
    /// שמירה, עם המרה של חריגות EF לחריגות שלנו, כדי ששכבת Application לא תכיר את EF.
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await Context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DuplicateEntryException(ex);
        }
    }
}
