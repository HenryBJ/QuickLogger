namespace QuickLogger.Domain.Dto;

public class LogList:Page
{
    public Guid AppId { get; set; }
    public Guid UserId { get; set; }
}
