using MongoDB.Driver;
using MongoDB.Driver.Linq;
using QuickLogger.Application.Interfaces;
using QuickLogger.Domain.Model;

namespace QuickLogger.Infrastructure.MongoDB;

public class LogRepository : IRepository<Log, Guid>
{
    private readonly IMongoCollection<Log> _logs;
    private readonly DataContext _dataContext;
    public LogRepository(DataContext context)
    {
        _logs = context.Logs;
        _dataContext = context;
    }

    public async Task<Log> AddAsync(Log entity)
    {
        await _logs.InsertOneAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<Log>> AddRangeAsync(IEnumerable<Log> entities)
    {
        await _logs.InsertManyAsync(entities);
        return entities;
    }

    public async Task<int> CountAsync()
    {
        return (int)await _logs.CountDocumentsAsync(FilterDefinition<Log>.Empty);
    }

    public async Task<int> CountAsync(Func<Log, bool> predicate)
    {
        var filter = Builders<Log>.Filter.Where(log => predicate(log));
        return (int)await _logs.CountDocumentsAsync(filter);
    }

    public async Task<bool> DeleteAsync(Log entity)
    {
        var result = await _logs.DeleteOneAsync(log => log.Id == entity.Id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        var result = await _logs.DeleteOneAsync(log => log.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> DeleteRangeAsync(IEnumerable<Log> entities)
    {
        var ids = entities.Select(e => e.Id).ToList();
        var filter = Builders<Log>.Filter.In(log => log.Id, ids);
        var result = await _logs.DeleteManyAsync(filter);
        return result.DeletedCount > 0;
    }

    public void Dispose()
    {
        // No disposable resources are used in this implementation, but you can add custom cleanup logic here if needed.
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var exists = await _logs.Find(log => log.Id == id).AnyAsync();
        return exists;
    }

    public async Task<bool> ExistsAsync(Func<Log, bool> predicate)
    {
        var log = await _logs.AsQueryable().FirstOrDefaultAsync(log => predicate(log));
        return log != null;
    }

    public async Task<IEnumerable<Log>> FindAsync(Func<Log, bool> predicate)
    {
        var logs = await _logs.AsQueryable().Where(log => predicate(log)).ToListAsync();
        return logs;
    }

    public async Task<IEnumerable<Log>> GetAllAsync()
    {
        return await _logs.Find(FilterDefinition<Log>.Empty).ToListAsync();
    }

    public async Task<Log?> GetByIdAsync(Guid id)
    {
        return await _logs.Find(log => log.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Log>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _logs.Find(FilterDefinition<Log>.Empty)
                          .Skip((pageNumber - 1) * pageSize)
                          .Limit(pageSize)
                          .ToListAsync();
    }

    public async Task<Log?> GetSingleAsync(Func<Log, bool> predicate)
    {
        return await _logs.AsQueryable().Where(log => predicate(log)).FirstOrDefaultAsync();
    }

    public Task<IEnumerable<Log>> GetSortedAsync<TKey>(Func<Log, TKey> keySelector, bool ascending = true, int pageNumber = 1, int pageSize = 10)
    {
        var query = _logs.AsQueryable();
        var sortedQuery = ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);

        return Task.FromResult(sortedQuery.Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize));
    }

    public async Task<IEnumerable<Log>> SearchAsync(Func<Log, bool> predicate)
    {
        return await _logs.AsQueryable().Where(log => predicate(log)).ToListAsync();
    }

    public async Task<IEnumerable<Log>> SearchWithPagingAsync(Func<Log, bool> predicate, int pageNumber, int pageSize)
    {
        return await _logs.AsQueryable()
                          .Where(log => predicate(log))
                          .Skip((pageNumber - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
    }

    public async Task<bool> UpdateAsync(Log entity)
    {
        var result = await _logs.ReplaceOneAsync(log => log.Id == entity.Id, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateRangeAsync(IEnumerable<Log> entities)
    {
        var bulkOps = new List<WriteModel<Log>>();

        foreach (var entity in entities)
        {
            var filter = Builders<Log>.Filter.Eq(log => log.Id, entity.Id);
            var update = Builders<Log>.Update.Set(log => log, entity);
            bulkOps.Add(new UpdateOneModel<Log>(filter, update));
        }

        var result = await _logs.BulkWriteAsync(bulkOps);
        return result.ModifiedCount > 0;
    }

    public async Task<IDatabaseTransaction> BeginTransactionAsyn()
    {
        return new MongoDatabaseTransaction(await _dataContext.Client.StartSessionAsync());
    }
}