

using QuickLogger.Domain.Model;

namespace QuickLogger.Application.Interfaces;

public interface IDatabaseHandlerFactory:IDisposable
{
    Task<IDatabaseHandler> CreateDatabaseHandlerAsync(DBItem dbItem);
    Task<IDatabaseHandler> GetLeastLoadedDatabaseHandlerAsync();
    Task<IDatabaseHandler?> GetDatabaseHandlerByAppAsync(Guid appId);
    Task<IEnumerable<IDatabaseHandler>> GetAllDatabaseHandlersAsync();
    
    /// <summary>
    /// Sincroniza todos los IDatabaseHandler para que tengan la misma lista de DBItems.
    /// </summary>
    Task SyncDatabaseHandlersAsync();
}
