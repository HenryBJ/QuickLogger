using QuickLogger.Domain.Model;

namespace QuickLogger.Application.Interfaces;

public interface IDatabaseHandler:IDisposable
{
    public bool IsSeed { get; set; }
    public bool IsActive { get; set; }
    Task<IRepository<App,Guid>> GetAppsRepositoryAsync();
    Task<IRepository<Log,Guid>> GetLogsRepositoryAsync();
    Task<IRepository<DBItem,Guid>> GetDBItemRepositoryAsync();
}
