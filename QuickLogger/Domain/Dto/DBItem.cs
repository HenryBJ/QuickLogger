namespace QuickLogger.Domain.Dto;

public class DBItem
{
    public string Name { get; set; } // mongodb, mssql, mysql..
    public string ConnectionString { get; set; }
    public bool IsSeed { get; set; }
    public string? Version { get; set; }
}
