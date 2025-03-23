using MongoDB.Driver;
using MongoDB.Driver.Linq;
using QuickLogger.Application.Interfaces;
using QuickLogger.Domain.Model;

namespace QuickLogger.Infrastructure.MongoDB;

public class AppRepository : IRepository<App, Guid>
{
    private readonly IMongoCollection<App> _apps;
    private readonly DataContext _dataContext;

    public AppRepository(DataContext context)
    {
        _apps = context.Apps;
        _dataContext = context;
    }

    public async Task<App> AddAsync(App entity)
    {
        await _apps.InsertOneAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<App>> AddRangeAsync(IEnumerable<App> entities)
    {
        await _apps.InsertManyAsync(entities);
        return entities;
    }

    public async Task<int> CountAsync()
    {
        return (int)await _apps.CountDocumentsAsync(FilterDefinition<App>.Empty);
    }

    public async Task<bool> DeleteAsync(App entity)
    {
        var result = await _apps.DeleteOneAsync(a => a.Id == entity.Id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        var result = await _apps.DeleteOneAsync(a => a.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<IEnumerable<App>> GetAllAsync()
    {
        return await _apps.Find(FilterDefinition<App>.Empty).ToListAsync();
    }

    public async Task<App?> GetByIdAsync(Guid id)
    {
        return await _apps.Find(a => a.Id == id).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateAsync(App entity)
    {
        var result = await _apps.ReplaceOneAsync(a => a.Id == entity.Id, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<IEnumerable<App>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _apps.Find(FilterDefinition<App>.Empty)
                          .Skip((pageNumber - 1) * pageSize)
                          .Limit(pageSize)
                          .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _apps.Find(a => a.Id == id).AnyAsync();
    }

    public async Task<App?> GetSingleAsync(Func<App, bool> predicate)
    {
        return await _apps.AsQueryable().Where(app => predicate(app)).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<App>> FindAsync(Func<App, bool> predicate)
    {
        return await _apps.AsQueryable().Where(app => predicate(app)).ToListAsync();
    }

    public Task<IEnumerable<App>> GetSortedAsync<TKey>(Func<App, TKey> keySelector, bool ascending = true, int pageNumber = 1, int pageSize = 10)
    {
        var query = _apps.AsQueryable();

        var newquery = ascending ? query.OrderBy(keySelector!) : query.OrderByDescending(keySelector);

        return Task.FromResult( newquery.Skip((pageNumber - 1) * pageSize)
                          .Take(pageSize));
    }

    public async Task<bool> UpdateRangeAsync(IEnumerable<App> entities)
    {
        var bulkOps = new List<WriteModel<App>>();

        foreach (var entity in entities)
        {
            var filter = Builders<App>.Filter.Eq(a => a.Id, entity.Id);
            var update = Builders<App>.Update.Set(a => a, entity);
            bulkOps.Add(new UpdateOneModel<App>(filter, update));
        }

        var result = await _apps.BulkWriteAsync(bulkOps);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteRangeAsync(IEnumerable<App> entities)
    {
        var ids = entities.Select(e => e.Id).ToList();
        var filter = Builders<App>.Filter.In(a => a.Id, ids);
        var result = await _apps.DeleteManyAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<int> CountAsync(Func<App, bool> predicate)
    {
        return await _apps.AsQueryable().CountAsync(app => predicate(app));
    }

    public async Task<bool> ExistsAsync(Func<App, bool> predicate)
    {
        return await _apps.AsQueryable().AnyAsync(app => predicate(app));
    }

    public async Task<IEnumerable<App>> SearchAsync(Func<App, bool> predicate)
    {
        return await _apps.AsQueryable().Where(app => predicate(app)).ToListAsync();
    }

    public Task<IEnumerable<App>> SearchWithPagingAsync(Func<App, bool> predicate, int pageNumber, int pageSize)
    {
        return Task.FromResult(_apps.AsQueryable()
                          .Where(app => predicate(app))
                          .Skip((pageNumber - 1) * pageSize)
                          .Take(pageSize).ToList() as IEnumerable<App>);
    }

    public void Dispose()
    {
        
    }

    public async Task<IDatabaseTransaction> BeginTransactionAsyn()
    {
         return new MongoDatabaseTransaction(await _dataContext.Client.StartSessionAsync());
    }
}