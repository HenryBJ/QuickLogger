using Microsoft.EntityFrameworkCore;
using QuickLogger.Domain.Model;

namespace QuickLogger.Infrastructure.MySql;

public class DataContext : DbContext
{
    private readonly string _connectionString;
    private readonly string _version;

    public DbSet<App> Apps { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<DBItem> DBItems { get; set; }

    public DataContext(string connectionString, string version="8.0.32")
    {
        _connectionString = connectionString;
        _version = version;
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(
            _connectionString,
            new MySqlServerVersion(new Version(_version))
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aquí puedes agregar configuraciones adicionales para las entidades si es necesario
    }
}
