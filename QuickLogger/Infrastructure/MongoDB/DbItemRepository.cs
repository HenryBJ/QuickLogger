using MongoDB.Driver;
using MongoDB.Driver.Linq;
using QuickLogger.Application.Interfaces;
using QuickLogger.Domain.Model;

namespace QuickLogger.Infrastructure.MongoDB;

public class DbItemRepository : IRepository<DBItem, Guid>
{
    private readonly IMongoCollection<DBItem> _dbItems;
    private readonly DataContext _dataContext;  
    public DbItemRepository(DataContext context)
    {
        _dbItems = context.DBItems;
        _dataContext = context;
    }

    public async Task<DBItem> AddAsync(DBItem entity)
    {
        await _dbItems.InsertOneAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<DBItem>> AddRangeAsync(IEnumerable<DBItem> entities)
    {
        await _dbItems.InsertManyAsync(entities);
        return entities;
    }

    public async Task<int> CountAsync()
    {
        return (int)await _dbItems.CountDocumentsAsync(FilterDefinition<DBItem>.Empty);
    }

    public async Task<int> CountAsync(Func<DBItem, bool> predicate)
    {
        var filter = Builders<DBItem>.Filter.Where(x => predicate(x));
        return (int)await _dbItems.CountDocumentsAsync(filter);
    }

    public async Task<bool> DeleteAsync(DBItem entity)
    {
        var result = await _dbItems.DeleteOneAsync(item => item.Id == entity.Id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        var result = await _dbItems.DeleteOneAsync(item => item.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> DeleteRangeAsync(IEnumerable<DBItem> entities)
    {
        var ids = entities.Select(e => e.Id).ToList();
        var filter = Builders<DBItem>.Filter.In(x => x.Id, ids);
        var result = await _dbItems.DeleteManyAsync(filter);
        return result.DeletedCount > 0;
    }

    public void Dispose()
    {
        // Dispose logic (if necessary, usually MongoClient does not need explicit disposal)
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var result = await _dbItems.Find(item => item.Id == id).AnyAsync();
        return result;
    }

    public async Task<bool> ExistsAsync(Func<DBItem, bool> predicate)
    {
        var result = await _dbItems.AsQueryable().AnyAsync(app => predicate(app));
        return result;
    }

    public async Task<IEnumerable<DBItem>> FindAsync(Func<DBItem, bool> predicate)
    {
        return await _dbItems.AsQueryable().Where(app => predicate(app)).ToListAsync();
    }

    public async Task<IEnumerable<DBItem>> GetAllAsync()
    {
        return await _dbItems.Find(FilterDefinition<DBItem>.Empty).ToListAsync();
    }

    public async Task<DBItem?> GetByIdAsync(Guid id)
    {
        return await _dbItems.Find(item => item.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<DBItem>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _dbItems.Find(FilterDefinition<DBItem>.Empty)
                              .Skip((pageNumber - 1) * pageSize)
                              .Limit(pageSize)
                              .ToListAsync();
    }

    public async Task<DBItem?> GetSingleAsync(Func<DBItem, bool> predicate)
    {
        return await _dbItems.AsQueryable().Where(app => predicate(app)).FirstOrDefaultAsync();
    }

    public Task<IEnumerable<DBItem>> GetSortedAsync<TKey>(Func<DBItem, TKey> keySelector, bool ascending = true, int pageNumber = 1, int pageSize = 10)
    {
        var query = _dbItems.AsQueryable();

        var sortedQuery = ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);

        return Task.FromResult(sortedQuery.Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize));
    }

    public async Task<IEnumerable<DBItem>> SearchAsync(Func<DBItem, bool> predicate)
    {
        return await _dbItems.AsQueryable().Where(app => predicate(app)).ToListAsync();
    }

    public Task<IEnumerable<DBItem>> SearchWithPagingAsync(Func<DBItem, bool> predicate, int pageNumber, int pageSize)
    {
        return Task.FromResult( _dbItems.AsQueryable()
                             .Where(predicate)
                             .Skip((pageNumber - 1) * pageSize)
                             .Take(pageSize));
    }

    public async Task<bool> UpdateAsync(DBItem entity)
    {
        var result = await _dbItems.ReplaceOneAsync(item => item.Id == entity.Id, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateRangeAsync(IEnumerable<DBItem> entities)
    {
        var bulkOps = new List<WriteModel<DBItem>>();

        foreach (var entity in entities)
        {
            var filter = Builders<DBItem>.Filter.Eq(x => x.Id, entity.Id);
            var update = Builders<DBItem>.Update.Set(x => x, entity);
            bulkOps.Add(new UpdateOneModel<DBItem>(filter, update));
        }

        var result = await _dbItems.BulkWriteAsync(bulkOps);
        return result.ModifiedCount > 0;
    }

    public async Task<IDatabaseTransaction> BeginTransactionAsyn()
    {
        return new MongoDatabaseTransaction(await _dataContext.Client.StartSessionAsync());
    }
}
