using Microsoft.AspNetCore.Mvc;
using QuickLogger.Application.Interfaces;
using QuickLogger.Domain.Dto;
using QuickLogger.Domain.Model;

namespace QuickLogger.Controllers;

[ApiController]
[Route("[controller]")]
public class AppController : ControllerBase
{
    private readonly ILogger<AppController> _logger;
    private readonly IDatabaseHandlerFactory _databaseHandlerFactory;

    public AppController(IDatabaseHandlerFactory databaseHandlerFactory, ILogger<AppController> logger)
    {
        _databaseHandlerFactory = databaseHandlerFactory;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] QuickLogger.Domain.Model.App data)
    {
        var dbhandler = await _databaseHandlerFactory.GetLeastLoadedDatabaseHandlerAsync();
        var repo = await dbhandler.GetAppsRepositoryAsync();

        var app = await repo.AddAsync(new Domain.Model.App 
        {
            Id = Guid.NewGuid(),
            Name = data.Name,
            UserId = data.UserId,
            Active = true,
            RetainDataPeriod = TimeSpan.FromDays(7),
            RegisterCritical = true,
            RegisterError = true,
            RegisterInfo = true,
            RegisterWarning = true
        });

        return Ok(new { id=app.Id });
    }

    [HttpPut]
    public async Task<IActionResult> Edit([FromBody] AppEdit data)
    {
        var dbhandler = await _databaseHandlerFactory.GetDatabaseHandlerByAppAsync(data.Id);
        if (dbhandler == null) return BadRequest(new { error = "App not found" });

        var repo = await dbhandler.GetAppsRepositoryAsync();

        var app = await repo.GetByIdAsync(data.Id);

        app!.Name = data.Name ?? app.Name;
        app.Active = data.Active ?? app.Active;
        app.RegisterCritical = data.RegisterCritical ?? app.RegisterCritical;
        app.RegisterError = data.RegisterError ?? app.RegisterError;
        app.RegisterInfo = data.RegisterInfo ?? app.RegisterInfo;
        app.RegisterWarning = data.RegisterWarning ?? app.RegisterWarning;

        await repo.UpdateAsync(app);

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] AppDelete data)
    {
        var dbhandler = await _databaseHandlerFactory.GetDatabaseHandlerByAppAsync(data.Id);
        if (dbhandler == null) return BadRequest(new { error = "App not found" });
        var repo = await dbhandler.GetAppsRepositoryAsync();

        var app = await repo.GetByIdAsync(data.Id);
        if (app == null) return BadRequest(new { error = "App not found" });
        if (app.UserId != data.UserId) return Unauthorized();

        await repo.DeleteAsync(app);

        return Ok();
    }

    //[HttpGet]
    //public async Task<IActionResult> List([FromQuery] AppList data)
    //{
    //    var dbhandlers = await _databaseHandlerFactory.GetAllDatabaseHandlersAsync();
    //    List<QuickLogger.Domain.Model.App> appsTotal = new List<QuickLogger.Domain.Model.App>();
        
    //    foreach (var dbhandler in dbhandlers)
    //    {
    //        var repo = await dbhandler.GetAppsRepositoryAsync();
    //        var apps = await repo.FindAsync(app => app.UserId == data.UserId);
    //        appsTotal.AddRange(apps);
    //    }

    //    return Ok(new { items = appsTotal });
    //}

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] AppList data)
    {
        var dbhandlers = await _databaseHandlerFactory.GetAllDatabaseHandlersAsync();

        // Crear tareas para obtener las apps de cada base de datos en paralelo
        var tasks = dbhandlers.Select(async dbhandler =>
        {
            var repo = await dbhandler.GetAppsRepositoryAsync();
            return await repo.FindAsync(app => app.UserId == data.UserId);
        });

        // Esperar todas las tareas y juntar los resultados
        var appsTotal = (await Task.WhenAll(tasks)).SelectMany(apps => apps).ToList();

        return Ok(new { items = appsTotal });
    }
}
