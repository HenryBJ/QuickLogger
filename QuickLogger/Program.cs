namespace QuickLogger;
using QuickLogger.Application.Interfaces;
using QuickLogger.Domain.Model;
using QuickLogger.Infrastructure.Common;
using QuickLogger.Rabbitmq;
using QuickLogger.Workers;
using Scalar.AspNetCore;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddSingleton<IDatabaseHandlerFactory>(_ =>
        {
            return new DatabaseHandlerFactory(new DBItem
            {
                Id = new Guid("d21d5d8c-0d88-40f6-a569-a50c0de0de7f"),
                Name = "mssql",
                IsSeed = true,// only for seed, do not save logs here
                Active = false,
                LastModified = DateTime.UtcNow,
                ConnectionString = builder.Configuration["quicklogger:seed"]!
            });
        });
        builder.Services.AddSingleton<RabbitMqConnection>();
        builder.Services.AddSingleton<LogRouter>();
        builder.Services.AddHostedService<LogConsumer>();
        builder.Services.AddHostedService<RemoveOldLogsWorker>();

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "QuickLogger";
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}