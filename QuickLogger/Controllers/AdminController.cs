using Microsoft.AspNetCore.Mvc;
using QuickLogger.Application.Interfaces;
using QuickLogger.Domain.Dto;
using QuickLogger.Domain.Model;
using QuickLogger.Infrastructure.Common;
using QuickLogger.Infrastructure.Utils;

namespace QuickLogger.Controllers;

[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IDatabaseHandlerFactory _databaseHandlerFactory;

    public AdminController(IDatabaseHandlerFactory databaseHandlerFactory, ILogger<AdminController> logger)
    {
        _databaseHandlerFactory = databaseHandlerFactory;
        _logger = logger;
    }

    [HttpPost("add-bditem")]
    public async Task<IActionResult> Add([FromBody] QuickLogger.Domain.Dto. DBItem data)
    {
        try
        {
            if (!new[] { "mssql", "mysql", "mongodb" }.Contains(data.Name.Trim().ToLower()))
                return BadRequest(new { error = "Invalid DB Provider Name" });


            string cxn = Base64Util.TryFromBase64String(data.ConnectionString, out string plain) ? plain : data.ConnectionString;
            if (data.Name.Trim().ToLower() == "mysql")
            {
                cxn = MySqlUtil.TryMySqlUrlToConnectionString(cxn, out string cstring) ? cstring : cxn;   
            }
            
            QuickLogger.Domain.Model.DBItem model = new QuickLogger.Domain.Model.DBItem
            {
                ConnectionString = cxn,
                Name = data.Name,
                Active = true,
                IsSeed = data.IsSeed,
                Id = Guid.NewGuid(),
                LastModified = DateTime.UtcNow,
                Version = data.Version
            };
            var dbhandler = await _databaseHandlerFactory.CreateDatabaseHandlerAsync(model);
            var repo = await dbhandler.GetDBItemRepositoryAsync();
            var item = await repo.AddAsync(model);

            await _databaseHandlerFactory.SyncDatabaseHandlersAsync();

            return Ok(new { id = item.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        
    }

    [HttpPut("edit-bditem")]
    public async Task<IActionResult> Edit([FromBody] DBItemEdit data)
    {
        try
        {
            var dbhandler = await _databaseHandlerFactory.GetLeastLoadedDatabaseHandlerAsync();
            var repo = await dbhandler.GetDBItemRepositoryAsync();

            var item = await repo.GetByIdAsync(data.Id);
            if (item == null) return BadRequest(new { error = "DBItem not found" });

            item.Name = data.Name ?? item.Name;
            item.Active = data.Active ?? item.Active;

            await repo.UpdateAsync(item);
            await _databaseHandlerFactory.SyncDatabaseHandlersAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        
    }

    [HttpDelete("delete-bditem")]
    public async Task<IActionResult> Delete([FromBody] QuickLogger.Domain.Dto.DBItemDelete data)
    {
        try
        {
            var dbhandlers = await _databaseHandlerFactory.GetAllDatabaseHandlersAsync();
            await Task.WhenAll(dbhandlers.Select(async handler =>
            {
                var repo = await handler.GetDBItemRepositoryAsync();
                if (await repo.ExistsAsync(data.Id))
                {
                    await repo.DeleteByIdAsync(data.Id);
                }
            }));
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("list-bditem")]
    public async Task<IActionResult> List()
    {
        try
        {
            var dbhandler = await _databaseHandlerFactory.GetLeastLoadedDatabaseHandlerAsync();
            var repo = await dbhandler.GetDBItemRepositoryAsync();
            var dbItems = await repo.GetAllAsync();
            return Ok(new { items = dbItems });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        
    }
}
