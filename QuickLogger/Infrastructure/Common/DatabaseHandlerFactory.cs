using QuickLogger.Application.Interfaces;
using QuickLogger.Domain.Model;
using QuickLogger.Infrastructure.Utils;
using System.Collections.Concurrent;

namespace QuickLogger.Infrastructure.Common;

public class DatabaseHandlerFactory : IDatabaseHandlerFactory
{
    private readonly DBItem _seed;
    private readonly ConcurrentDictionary<Guid, IDatabaseHandler> _dbHandlers;
    private readonly ConcurrentDictionary<Guid, IDatabaseHandler?> _appIdToHandlerCache; // Cache de appId -> DBHandler

    public DatabaseHandlerFactory(DBItem seed)
    {
        _seed = seed;
        _dbHandlers = new ConcurrentDictionary<Guid, IDatabaseHandler>();
        _appIdToHandlerCache = new ConcurrentDictionary<Guid, IDatabaseHandler?>();
        LoadDatabaseHandlersAsync(_seed).Wait();
    }

    /// <summary>
    /// Carga todos los DBItems del primer dbhandler creado con el DBItem seed y crea los demás dbhandlers.
    /// </summary>
    private async Task LoadDatabaseHandlersAsync(DBItem seed)
    {
        var firstHandler = new DatabaseHandler(seed);
        _dbHandlers.TryAdd(seed.Id, firstHandler);

        var repo = await firstHandler.GetDBItemRepositoryAsync();
        var dbItems = await repo.GetAllAsync();

        var tasks = dbItems
            .Where(dbItem => dbItem.Id != _seed.Id) // Evita duplicar el `seed`
            .Select(async dbItem =>
            {
                var handler = new DatabaseHandler(dbItem);
                _dbHandlers.TryAdd(dbItem.Id, handler);
            });

        await Task.WhenAll(tasks);
    }

    public Task<IDatabaseHandler> CreateDatabaseHandlerAsync(DBItem dbItem)
    {
        var handler = new DatabaseHandler(dbItem);
        _dbHandlers.TryAdd(dbItem.Id, handler);
        return Task.FromResult<IDatabaseHandler>(handler);
    }

    public void Dispose()
    {
        foreach (var handler in _dbHandlers.Values)
        {
            handler.Dispose();
        }
        _dbHandlers.Clear();
    }

    public Task<IEnumerable<IDatabaseHandler>> GetAllDatabaseHandlersAsync()
    {
        return Task.FromResult<IEnumerable<IDatabaseHandler>>(_dbHandlers.Values);
    }

    /// <summary>
    /// Obtiene el DatabaseHandler correspondiente a un appId. Usa caché para mejorar el rendimiento.
    /// </summary>
    public async Task<IDatabaseHandler?> GetDatabaseHandlerByAppAsync(Guid appId)
    {

        // Verifica si el appId ya está en caché
        if (_appIdToHandlerCache.TryGetValue(appId, out var cachedHandler))
        {
            return cachedHandler;
        }

        foreach (var handler in _dbHandlers.Values)
        {
            var appRepo = await handler.GetAppsRepositoryAsync();
            var app = await appRepo.GetByIdAsync(appId);
            if (app != null)
            {
                _appIdToHandlerCache.TryAdd(appId, handler); // Guarda en caché
                return handler;
            }
        }
        return null;
    }

    public async Task<IDatabaseHandler> GetLeastLoadedDatabaseHandlerAsync()
    {
        if (!_dbHandlers.Any())
            throw new InvalidOperationException("No database handlers available.");

        var handlerLoadTasks = _dbHandlers.Values.Select(async handler =>
        {
            var appRepo = await handler.GetAppsRepositoryAsync();
            var count = await appRepo.CountAsync();
            return (handler, count);
        });
        var handlerLoads = await Task.WhenAll(handlerLoadTasks);
        return handlerLoads
            .Where(h=>!h.handler.IsSeed)
            .Where(h => h.handler.IsActive)
            .OrderBy(h => h.count).First().handler;
    }
    
    /// <summary>
    /// ✅ Paso 1: Obtener todos los DBItems de todas las bases de datos.
    /// ✅ Paso 2: Fusionar los resultados, eliminando duplicados.
    /// ✅ Paso 3: Insertar en cada base de datos los DBItems que le falten.
    /// </summary>
    /// <returns></returns>
    public async Task SyncDatabaseHandlersAsync()
    {
        if (_dbHandlers.Count == 0)
            return;

        var allDbItems = new HashSet<QuickLogger.Domain.Model.DBItem>(new DBItemComparer());

        // Paso 1: Obtener todos los DBItems de cada base de datos
        foreach (var handler in _dbHandlers.Values)
        {
            var repo = await handler.GetDBItemRepositoryAsync();
            var dbItems = await repo.GetAllAsync();

            foreach (var dbItem in dbItems)
            {
                allDbItems.Add(dbItem);
            }
        }

        // Paso 2 y 3: Insertar los DBItems que falten en cada base de datos
        foreach (var handler in _dbHandlers.Values)
        {
            var repo = await handler.GetDBItemRepositoryAsync();
            var existingDbItems = await repo.GetAllAsync();

            var missingDbItems = allDbItems
                .Where(globalItem => !existingDbItems.Any(db => db.Id == globalItem.Id))
                .ToList();

            if (missingDbItems.Count > 0)
            {
                await SyncDatabaseItems(repo, missingDbItems);
            }
        }
    }

    private async Task SyncDatabaseItems(IRepository<DBItem, Guid> repo, List<DBItem> missingDbItems)
    {
        await using var transaction = await repo.BeginTransactionAsyn();

        try
        {
            await repo.AddRangeAsync(missingDbItems);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
