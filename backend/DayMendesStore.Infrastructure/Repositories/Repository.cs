using System.Linq.Expressions;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Domain.Entities;
using DayMendesStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DayMendesStore.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<T?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (_context.Database.IsRelational())
        {
            var tableName = _context.Model.FindEntityType(typeof(T))?.GetTableName() ?? typeof(T).Name;
            var sql = $"SELECT * FROM `{tableName}` WHERE `Id` = {id} FOR UPDATE";
            return await _dbSet.FromSqlRaw(sql).FirstOrDefaultAsync(cancellationToken);
        }

        var fresh = await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (fresh != null)
        {
            var local = _dbSet.Local.FirstOrDefault(e => e.Id == id);
            if (local != null)
            {
                _context.Entry(local).CurrentValues.SetValues(fresh);
                return local;
            }
            _dbSet.Attach(fresh);
            return fresh;
        }

        return null;
    }

    public virtual async Task<List<T>> GetByIdsForUpdateAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().OrderBy(i => i).ToList();
        if (!idList.Any())
        {
            return new List<T>();
        }

        if (_context.Database.IsRelational())
        {
            var tableName = _context.Model.FindEntityType(typeof(T))?.GetTableName() ?? typeof(T).Name;
            var inClause = string.Join(", ", idList);
            var sql = $"SELECT * FROM `{tableName}` WHERE `Id` IN ({inClause}) ORDER BY `Id` FOR UPDATE";
            return await _dbSet.FromSqlRaw(sql).ToListAsync(cancellationToken);
        }

        var freshList = await _dbSet.AsNoTracking().Where(e => idList.Contains(e.Id)).ToListAsync(cancellationToken);
        var result = new List<T>();
        foreach (var fresh in freshList)
        {
            var local = _dbSet.Local.FirstOrDefault(e => e.Id == fresh.Id);
            if (local != null)
            {
                _context.Entry(local).CurrentValues.SetValues(fresh);
                result.Add(local);
            }
            else
            {
                _dbSet.Attach(fresh);
                result.Add(fresh);
            }
        }
        return result.OrderBy(e => e.Id).ToList();
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    public virtual IQueryable<T> QueryIgnoreFilters()
    {
        return _dbSet.IgnoreQueryFilters().AsQueryable();
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }
}
