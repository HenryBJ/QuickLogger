using QuickLogger.Application.Interfaces;
using QuickLogger.Domain.Model;
using System.Collections.Concurrent;

namespace QuickLogger.Infrastructure.Common;

public class DatabaseHandler : IDatabaseHandler
{
    private readonly DBItem _dbItem;
    private readonly QuickLogger.Infrastructure.MongoDB.DataContext _mongodb;
    private readonly QuickLogger.Infrastructure.MsSql.DataContext _mssql;
    private readonly QuickLogger.Infrastructure.MySql.DataContext _mysql;

    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public bool IsSeed { get; set; }
    public bool IsActive { get; set; }
    public DatabaseHandler(DBItem dbItem)
    {
        IsSeed = dbItem.IsSeed;
        IsActive = dbItem.Active;
        _dbItem = dbItem;
        switch (dbItem.Name.ToLower())
        {
            case "mongodb":
                _mongodb = new QuickLogger.Infrastructure.MongoDB.DataContext(_dbItem.ConnectionString);
                break;
            case "mssql":
                _mssql = new QuickLogger.Infrastructure.MsSql.DataContext(_dbItem.ConnectionString);
                break;
            case "mysql":
                _mysql = string.IsNullOrEmpty(_dbItem.Version) ?
                    new QuickLogger.Infrastructure.MySql.DataContext(_dbItem.ConnectionString) :
                    new QuickLogger.Infrastructure.MySql.DataContext(_dbItem.ConnectionString, _dbItem.Version);
                break;
            default:
                throw new ArgumentException("Invalid Database Type");
        }
    }

    public void Dispose()
    {
        _mssql?.Dispose();
        _mysql?.Dispose();
    }

    public Task<IRepository<App, Guid>> GetAppsRepositoryAsync()
    {
        return Task.FromResult((IRepository<App, Guid>)_repositories.GetOrAdd(typeof(IRepository<App, Guid>), _ =>
        {
            return _dbItem.Name.ToLower() switch
            {
                "mongodb" => new QuickLogger.Infrastructure.MongoDB.AppRepository(_mongodb),
                "mssql" => new QuickLogger.Infrastructure.MsSql.AppRepository(_mssql),
                "mysql" => new QuickLogger.Infrastructure.MySql.AppRepository(_mysql),
                _ => throw new ArgumentException("Invalid Database Type")
            };
        }));
    }

    public Task<IRepository<DBItem, Guid>> GetDBItemRepositoryAsync()
    {
        return Task.FromResult((IRepository<DBItem, Guid>)_repositories.GetOrAdd(typeof(IRepository<DBItem, Guid>), _ =>
        {
            return _dbItem.Name.ToLower() switch
            {
                "mongodb" => new QuickLogger.Infrastructure.MongoDB.DbItemRepository(_mongodb),
                "mssql" => new QuickLogger.Infrastructure.MsSql.DbItemRepository(_mssql),
                "mysql" => new QuickLogger.Infrastructure.MySql.DbItemRepository(_mysql),
                _ => throw new ArgumentException("Invalid Database Type")
            };
        }));
    }

    public Task<IRepository<Log, Guid>> GetLogsRepositoryAsync()
    {
        return Task.FromResult((IRepository<Log, Guid>)_repositories.GetOrAdd(typeof(IRepository<Log, Guid>), _ =>
        {
            return _dbItem.Name.ToLower() switch
            {
                "mongodb" => new QuickLogger.Infrastructure.MongoDB.LogRepository(_mongodb),
                "mssql" => new QuickLogger.Infrastructure.MsSql.LogRepository(_mssql),
                "mysql" => new QuickLogger.Infrastructure.MySql.LogRepository(_mysql),
                _ => throw new ArgumentException("Invalid Database Type")
            };
        }));
    }
}