namespace QuickLogger.Application.Interfaces;

public interface IDatabaseTransaction : IAsyncDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
