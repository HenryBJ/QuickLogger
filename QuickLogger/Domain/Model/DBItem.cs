using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace QuickLogger.Domain.Model;

public class DBItem
{
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
    public string Name { get; set; } // MongoDB, MSSQL, MYSQL
    public string ConnectionString { get; set; }
    public string? Version { get; set; }
    public bool IsSeed { get; set; }
    public bool Active { get; set; } // Si es false no debe ser elegido para nuevas apps
    public DateTime LastModified { get; set; }
}
