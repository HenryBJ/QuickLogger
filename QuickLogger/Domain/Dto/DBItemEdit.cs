namespace QuickLogger.Domain.Dto;

public class DBItemEdit
{
    public Guid Id { get; set; }
    public string Name { get; set; } // MongoDB, MSSQL, MYSQL
    public bool? Active { get; set; }
}
