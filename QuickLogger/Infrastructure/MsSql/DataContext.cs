using Microsoft.EntityFrameworkCore;
using QuickLogger.Domain.Model;

namespace QuickLogger.Infrastructure.MsSql;

public class DataContext:DbContext
{
    private readonly string _connectionString;

    public DbSet<App> Apps { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<DBItem> DBItems { get; set; }

    public DataContext(string connectionString)
    {
        _connectionString = connectionString;
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aquí podrías agregar configuraciones específicas para tus entidades
    }
}
