using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PurchaseTicket.Infrastructure.Persistence;

namespace PurchaseTicket.Infrastructure.Tests;

public abstract class InfrastructureTestBase : IAsyncLifetime
{
    protected ApplicationDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var options = CreateDbContextOptions();

        Context = new ApplicationDbContext(options);

        await Context.Database.MigrateAsync();

        await CleanDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
    }

    private static DbContextOptions<ApplicationDbContext>
        CreateDbContextOptions()
    {
        LoadEnvironmentVariables();

        var host = GetRequiredEnvironmentVariable(
            "POSTGRES_HOST");

        var port = GetRequiredEnvironmentVariable(
            "POSTGRES_PORT");

        var username = GetRequiredEnvironmentVariable(
            "POSTGRES_USER");

        var password = GetRequiredEnvironmentVariable(
            "POSTGRES_PASSWORD");

        var connectionString =
            new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = int.Parse(port),
                Database = "purchase_ticket_tests",
                Username = username,
                Password = password
            }
            .ConnectionString;

        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;
    }

    private static void LoadEnvironmentVariables()
    {
        var directory =
            new DirectoryInfo(
                Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            var envPath =
                Path.Combine(directory.FullName, ".env");

            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                return;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            "Could not find the .env file.");
    }

    private static string GetRequiredEnvironmentVariable(
        string name)
    {
        return Environment.GetEnvironmentVariable(name)
            ?? throw new InvalidOperationException(
                $"Environment variable '{name}' is not configured.");
    }

    private async Task CleanDatabaseAsync()
    {
        await Context.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE purchase_tickets, suppliers, materials
            RESTART IDENTITY CASCADE;
            """);
    }
}