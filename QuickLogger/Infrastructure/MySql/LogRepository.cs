using Microsoft.EntityFrameworkCore;
using QuickLogger.Application.Interfaces;
using QuickLogger.Domain.Model;

namespace QuickLogger.Infrastructure.MySql;

public class LogRepository : IRepository<Log, Guid>
{
    private readonly DataContext _context;

    public LogRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Log> AddAsync(Log entity)
    {
        await _context.Logs.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<Log>> AddRangeAsync(IEnumerable<Log> entities)
    {
        await _context.Logs.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public async Task<int> CountAsync()
    {
        return await _context.Logs.CountAsync();
    }

    public async Task<int> CountAsync(Func<Log, bool> predicate)
    {
        return await Task.FromResult(_context.Logs.Count(predicate));
    }

    public async Task<bool> DeleteAsync(Log entity)
    {
        _context.Logs.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        var entity = await _context.Logs.FindAsync(id);
        if (entity == null) return false;
        _context.Logs.Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteRangeAsync(IEnumerable<Log> entities)
    {
        _context.Logs.RemoveRange(entities);
        return await _context.SaveChangesAsync() > 0;
    }

    public void Dispose()
    {
        _context.Dispose(); 
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Logs.AnyAsync(l => l.Id == id);
    }

    public async Task<bool> ExistsAsync(Func<Log, bool> predicate)
    {
        return await Task.FromResult(_context.Logs.Any(predicate));
    }

    public async Task<IEnumerable<Log>> FindAsync(Func<Log, bool> predicate)
    {
        return await Task.FromResult(_context.Logs.Where(predicate).ToList());
    }

    public async Task<IEnumerable<Log>> GetAllAsync()
    {
        return await _context.Logs.ToListAsync();
    }

    public async Task<Log?> GetByIdAsync(Guid id)
    {
        return await _context.Logs.FindAsync(id);
    }

    public async Task<IEnumerable<Log>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _context.Logs
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Log?> GetSingleAsync(Func<Log, bool> predicate)
    {
        return await Task.FromResult(_context.Logs.FirstOrDefault(predicate));
    }

    public Task<IEnumerable<Log>> GetSortedAsync<TKey>(Func<Log, TKey> keySelector, bool ascending = true, int pageNumber = 1, int pageSize = 10)
    {
        var query = ascending
            ? _context.Logs.OrderBy(keySelector)
            : _context.Logs.OrderByDescending(keySelector);

        return Task.FromResult(query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize));
    }

    public async Task<IEnumerable<Log>> SearchAsync(Func<Log, bool> predicate)
    {
        return await Task.FromResult(_context.Logs.Where(predicate).ToList());
    }

    public async Task<IEnumerable<Log>> SearchWithPagingAsync(Func<Log, bool> predicate, int pageNumber, int pageSize)
    {
        return await Task.FromResult(
            _context.Logs.Where(predicate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList()
        );
    }

    public async Task<bool> UpdateAsync(Log entity)
    {
        _context.Logs.Update(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateRangeAsync(IEnumerable<Log> entities)
    {
        _context.Logs.UpdateRange(entities);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IDatabaseTransaction> BeginTransactionAsyn()
    {
        return new SqlDatabaseTransaction(await _context.Database.BeginTransactionAsync());
    }
}