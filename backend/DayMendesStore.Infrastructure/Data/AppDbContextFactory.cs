using DayMendesStore.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DayMendesStore.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        EnvLoader.Load();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
        
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
            var database = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "day_mendes_store";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

            connectionString = $"Server={server};Port={port};Database={database};User={user};Password={password};";
        }

        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new AppDbContext(optionsBuilder.Options);
    }
}
