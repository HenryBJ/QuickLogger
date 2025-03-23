using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace QuickLogger.Domain.Model;

public class App
{
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool Active { get; set; }
    public bool RegisterInfo { get; set; }
    public bool RegisterWarning { get; set; }
    public bool RegisterError { get; set; }
    public bool RegisterCritical { get; set; }
    public TimeSpan RetainDataPeriod { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserId { get; set; }

}
