namespace QuickLogger.Domain.Dto;

public class Log
{
    public string Level { get; set; } = "info"; // Info, Warning, Error, Critical
    public string Message { get; set; }
    public DateTime DateTime { get; set; }
    public Guid AppId { get; set; }
    public string? Version { get; set; }
    public string? Environment { get; set; }
    public string? System { get; set; }
    public string? Platform { get; set; }
    public string? Lang { get; set; }
    public string? DeviceId { get; set; }
    public string? IPAddress { get; set; }
    public decimal? Mem { get; set; }
    public decimal? CPU { get; set; }
    public decimal? MemPercent { get; set; }
    public decimal? CPUPercent { get; set; }
    public string? UserId { get; set; }
    public string? UserRol { get; set; }
    public string? Action { get; set; }
    public string? StackTrace { get; set; }
    public string? Tag { get; set; }
}
