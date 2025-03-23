using QuickLogger.Application.Interfaces;
using QuickLogger.Domain.Dto;

namespace QuickLogger.Infrastructure.Common;

public class LogRouter : ILogRouter
{
    private readonly ILogger<LogRouter> _logger;
    private readonly IDatabaseHandlerFactory _dbHandlerFactory;

    public LogRouter(IDatabaseHandlerFactory dbHandlerFactory, ILogger<LogRouter> logger)
    {
        _dbHandlerFactory = dbHandlerFactory;
        _logger = logger;
    }

    /// <summary>
    /// Procesa un log y lo asigna al dbhandler correspondiente.
    /// Si el AppId no tiene un dbhandler, el log es ignorado.
    /// </summary>
    public async Task RouteLogAsync(Log log)
    {
        var dbHandler = await _dbHandlerFactory.GetDatabaseHandlerByAppAsync(log.AppId);

        if (dbHandler != null && dbHandler.IsActive)
        {
            var apprepo = await dbHandler.GetAppsRepositoryAsync();
            var app = await apprepo.GetByIdAsync(log.AppId);

            // cases to ignore
            if (!app!.Active) return;
            if (!app!.RegisterError && log.Level.ToLower() == "error") return;
            if (!app!.RegisterInfo && log.Level.ToLower() == "info") return;
            if (!app!.RegisterWarning && log.Level.ToLower() == "warning") return;
            if (!app!.RegisterCritical && log.Level.ToLower() == "critical") return;

            var repo = await dbHandler.GetLogsRepositoryAsync();
            await repo.AddAsync(new Domain.Model.Log
            {
                Id = Guid.NewGuid(),
                AppId = log.AppId,
                Message = log.Message,
                DateTime = DateTime.UtcNow,
                UserId = log.UserId,
                Action = log.Action,
                CPU = log.CPU,
                CPUPercent = log.CPUPercent,
                DeviceId = log.DeviceId,
                Environment = log.Environment,
                IPAddress = log.IPAddress,
                Lang = log.Lang,
                Level = log.Level,
                Mem = log.Mem, 
                MemPercent = log.MemPercent,    
                Platform = log.Platform,   
                StackTrace = log.StackTrace,
                System = log.System,
                Tag = log.Tag,
                UserRol = log.UserRol,
                Version = log.Version   
            });
        }
        // Si dbHandler es null, el log se ignora.
    }
}
