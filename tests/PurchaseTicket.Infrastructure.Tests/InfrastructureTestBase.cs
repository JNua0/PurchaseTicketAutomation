using Microsoft.EntityFrameworkCore;
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
        var port =
            Environment.GetEnvironmentVariable("POSTGRES_PORT");

        var username =
            Environment.GetEnvironmentVariable("POSTGRES_USER");

        var password =
            Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        var connectionString =
            $"Host=localhost;" +
            $"Port={port};" +
            $"Database=purchase_ticket_tests;" +
            $"Username={username};" +
            $"Password={password}";

        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;
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