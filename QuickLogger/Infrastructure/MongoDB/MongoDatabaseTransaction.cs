using MongoDB.Driver;
using QuickLogger.Application.Interfaces;

namespace QuickLogger.Infrastructure.MongoDB;

public class MongoDatabaseTransaction : IDatabaseTransaction
{
    private readonly IClientSessionHandle _session;

    public MongoDatabaseTransaction(IClientSessionHandle session)
    {
        _session = session;
        _session.StartTransaction();
    }

    public Task CommitAsync() => _session.CommitTransactionAsync();
    public Task RollbackAsync() => _session.AbortTransactionAsync();

    public ValueTask DisposeAsync()
    {
        _session.Dispose();
        return ValueTask.CompletedTask;
    }
}

