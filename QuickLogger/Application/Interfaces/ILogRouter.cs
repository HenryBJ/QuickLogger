using QuickLogger.Domain.Dto;

namespace QuickLogger.Application.Interfaces;

public interface ILogRouter
{
    Task RouteLogAsync(Log log);
}
