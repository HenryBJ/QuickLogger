using Microsoft.EntityFrameworkCore;
using QuickLogger.Application.Interfaces;
using QuickLogger.Domain.Model;

namespace QuickLogger.Infrastructure.MySql;

public class AppRepository : IRepository<App, Guid>
{
    private readonly DataContext _context;

    public AppRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<App> AddAsync(App entity)
    {
        await _context.Apps.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<App>> AddRangeAsync(IEnumerable<App> entities)
    {
        await _context.Apps.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public async Task<int> CountAsync()
    {
        return await _context.Apps.CountAsync();
    }

    public async Task<int> CountAsync(Func<App, bool> predicate)
    {
        return await Task.FromResult(_context.Apps.Count(predicate));
    }

    public async Task<bool> DeleteAsync(App entity)
    {
        _context.Apps.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        var entity = await _context.Apps.FindAsync(id);
        if (entity == null) return false;
        _context.Apps.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteRangeAsync(IEnumerable<App> entities)
    {
        _context.Apps.RemoveRange(entities);
        return await _context.SaveChangesAsync() > 0;
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Apps.AnyAsync(a => a.Id == id);
    }

    public async Task<bool> ExistsAsync(Func<App, bool> predicate)
    {
        return await Task.FromResult(_context.Apps.Any(predicate));
    }

    public async Task<IEnumerable<App>> FindAsync(Func<App, bool> predicate)
    {
        return await Task.FromResult(_context.Apps.Where(predicate).ToList());
    }

    public async Task<IEnumerable<App>> GetAllAsync()
    {
        return await _context.Apps.ToListAsync();
    }

    public async Task<App?> GetByIdAsync(Guid id)
    {
        return await _context.Apps.FindAsync(id);
    }

    public async Task<IEnumerable<App>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _context.Apps
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<App?> GetSingleAsync(Func<App, bool> predicate)
    {
        return await Task.FromResult(_context.Apps.FirstOrDefault(predicate));
    }

    public Task<IEnumerable<App>> GetSortedAsync<TKey>(Func<App, TKey> keySelector, bool ascending = true, int pageNumber = 1, int pageSize = 10)
    {
        var query = ascending
            ? _context.Apps.OrderBy(keySelector)
            : _context.Apps.OrderByDescending(keySelector);

        return Task.FromResult(query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize));
    }

    public async Task<IEnumerable<App>> SearchAsync(Func<App, bool> predicate)
    {
        return await Task.FromResult(_context.Apps.Where(predicate).ToList());
    }

    public async Task<IEnumerable<App>> SearchWithPagingAsync(Func<App, bool> predicate, int pageNumber, int pageSize)
    {
        return await Task.FromResult(
            _context.Apps.Where(predicate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList()
        );
    }

    public async Task<bool> UpdateAsync(App entity)
    {
        _context.Apps.Update(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateRangeAsync(IEnumerable<App> entities)
    {
        _context.Apps.UpdateRange(entities);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IDatabaseTransaction> BeginTransactionAsyn()
    {
        return new SqlDatabaseTransaction(await _context.Database.BeginTransactionAsync());
    }
}
