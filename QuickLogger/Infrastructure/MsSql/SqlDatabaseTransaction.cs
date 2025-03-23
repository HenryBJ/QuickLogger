using Microsoft.EntityFrameworkCore.Storage;
using QuickLogger.Application.Interfaces;

namespace QuickLogger.Infrastructure.MsSql;

public class SqlDatabaseTransaction : IDatabaseTransaction
{
    private readonly IDbContextTransaction _transaction;

    public SqlDatabaseTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task CommitAsync() => _transaction.CommitAsync();
    public Task RollbackAsync() => _transaction.RollbackAsync();

    public async ValueTask DisposeAsync() => await _transaction.DisposeAsync();
}
