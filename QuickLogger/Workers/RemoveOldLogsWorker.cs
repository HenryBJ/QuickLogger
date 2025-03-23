
using QuickLogger.Application.Interfaces;

namespace QuickLogger.Workers;

public class RemoveOldLogsWorker : BackgroundService
{
    private readonly IDatabaseHandlerFactory _databaseHandlerFactory;
    private readonly ILogger<RemoveOldLogsWorker> _logger;

    public RemoveOldLogsWorker(ILogger<RemoveOldLogsWorker> logger, IDatabaseHandlerFactory databaseHandlerFactory)
    {
        _logger = logger;
        _databaseHandlerFactory = databaseHandlerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunTaskAsync(stoppingToken); // Ejecuta la tarea principal

                // Calcula el tiempo hasta la próxima ejecución (mañana a la misma hora)
                var nextRun = DateTime.UtcNow.Date.AddDays(1).AddHours(0); // A las 00:00 UTC
                var delay = nextRun - DateTime.UtcNow;

                _logger.LogInformation($"Próxima ejecución en {delay.TotalHours} horas.");
                await Task.Delay(delay, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en RemoveOldLogsWorker");
            }
        }
    }

    private async Task RunTaskAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ejecutando limpieza de logs expirados...");
        try
        {
            var dbhandlers = await _databaseHandlerFactory.GetAllDatabaseHandlersAsync();

            await Task.WhenAll(dbhandlers.Select(async handler =>
            {
                var repoApps = await handler.GetAppsRepositoryAsync();
                var repoLogs = await handler.GetLogsRepositoryAsync();

                foreach (var app in await repoApps.GetAllAsync())
                {
                    var expirationDate = DateTime.UtcNow - app.RetainDataPeriod;
                    if (!await repoLogs.ExistsAsync(l => l.AppId == app.Id && l.DateTime < expirationDate))
                        continue;

                    _logger.LogInformation($"Eliminando logs expirados para la app {app.Name} ({app.Id}).");

                    await using var transaction = await repoLogs.BeginTransactionAsyn();
                    try
                    {
                        var expiredLogs = await repoLogs.SearchAsync(l => l.AppId == app.Id && l.DateTime < expirationDate);
                        await repoLogs.DeleteRangeAsync(expiredLogs);
                        await transaction.CommitAsync();
                        _logger.LogInformation($"Logs eliminados para la app {app.Name}.");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, $"Error al eliminar logs para {app.Name}.");
                    }
                }
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar logs expirados");
        }
    }
}
