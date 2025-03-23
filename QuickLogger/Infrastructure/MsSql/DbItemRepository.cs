using Microsoft.EntityFrameworkCore;
using QuickLogger.Application.Interfaces;
using QuickLogger.Domain.Model;

namespace QuickLogger.Infrastructure.MsSql;

public class DbItemRepository : IRepository<DBItem, Guid>
{
    private readonly DataContext _context;

    public DbItemRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<DBItem> AddAsync(DBItem entity)
    {
        _context.DBItems.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<DBItem>> AddRangeAsync(IEnumerable<DBItem> entities)
    {
        await _context.DBItems.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public async Task<int> CountAsync()
    {
        return await _context.DBItems.CountAsync();
    }

    public Task<int> CountAsync(Func<DBItem, bool> predicate)
    {
        return Task.FromResult(_context.DBItems.Count(predicate));
    }

    public async Task<bool> DeleteAsync(DBItem entity)
    {
        _context.DBItems.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        var entity = await _context.DBItems.FindAsync(id);
        if (entity == null) return false;
        _context.DBItems.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteRangeAsync(IEnumerable<DBItem> entities)
    {
        _context.DBItems.RemoveRange(entities);
        return await _context.SaveChangesAsync() > 0;
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.DBItems.AnyAsync(d => d.Id == id);
    }

    public Task<bool> ExistsAsync(Func<DBItem, bool> predicate)
    {
        return Task.FromResult(_context.DBItems.Any(predicate));
    }

    public async Task<IEnumerable<DBItem>> FindAsync(Func<DBItem, bool> predicate)
    {
        return await Task.FromResult(_context.DBItems.Where(predicate));
    }

    public async Task<IEnumerable<DBItem>> GetAllAsync()
    {
        return await _context.DBItems.ToListAsync();
    }

    public async Task<DBItem?> GetByIdAsync(Guid id)
    {
        return await _context.DBItems.FindAsync(id);
    }

    public async Task<IEnumerable<DBItem>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _context.DBItems.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<DBItem?> GetSingleAsync(Func<DBItem, bool> predicate)
    {
        return await Task.FromResult(_context.DBItems.FirstOrDefault(predicate));
    }

    public async Task<IEnumerable<DBItem>> GetSortedAsync<TKey>(Func<DBItem, TKey> keySelector, bool ascending = true, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.DBItems.AsQueryable();
        query = ascending ? query.OrderBy(keySelector).AsQueryable() : query.OrderByDescending(keySelector).AsQueryable();
        return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<IEnumerable<DBItem>> SearchAsync(Func<DBItem, bool> predicate)
    {
        return await Task.FromResult(_context.DBItems.Where(predicate));
    }

    public async Task<IEnumerable<DBItem>> SearchWithPagingAsync(Func<DBItem, bool> predicate, int pageNumber, int pageSize)
    {
        return await Task.FromResult(_context.DBItems.Where(predicate).Skip((pageNumber - 1) * pageSize).Take(pageSize));
    }

    public async Task<bool> UpdateAsync(DBItem entity)
    {
        _context.DBItems.Update(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateRangeAsync(IEnumerable<DBItem> entities)
    {
        _context.DBItems.UpdateRange(entities);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IDatabaseTransaction> BeginTransactionAsyn()
    {
        return new SqlDatabaseTransaction(await _context.Database.BeginTransactionAsync());
    }
}
