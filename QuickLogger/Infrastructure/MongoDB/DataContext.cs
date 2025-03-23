using MongoDB.Driver;
using QuickLogger.Domain.Model;

namespace QuickLogger.Infrastructure.MongoDB;

public class DataContext
{
    private readonly IMongoDatabase _database;
    public MongoClient Client { get; private set;} 

    public DataContext(string connectionString)
    {
        string database = "quicklogger";
        var client = new MongoClient(connectionString);
        Client = client;
        _database = client.GetDatabase(database);
    }

    public IMongoCollection<Log> Logs => _database.GetCollection<Log>("Logs");
    public IMongoCollection<DBItem> DBItems => _database.GetCollection<DBItem>("DBItems");
    public IMongoCollection<App> Apps => _database.GetCollection<App>("Apps");
}