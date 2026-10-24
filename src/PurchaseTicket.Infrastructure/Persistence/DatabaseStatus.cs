using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Application.Abstractions.Persistence;

namespace PurchaseTicket.Infrastructure.Persistence;

public sealed class DatabaseStatus : IDatabaseStatus
{
    // Campos

    private readonly ApplicationDbContext _context;

    // Constructor

    public DatabaseStatus(ApplicationDbContext context)
    {
        _context = context;
    }

    // Metodos publicos

    public async Task<bool> CanConnectAsync(TimeSpan timeout)
    {
        using var cancellation = new CancellationTokenSource(timeout);

        try
        {
            return await _context.Database.CanConnectAsync(cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<string>> GetPendingMigrationsAsync()
    {
        return (await _context.Database.GetPendingMigrationsAsync()).ToList();
    }

    public async Task<int> CountAppliedMigrationsAsync()
    {
        return (await _context.Database.GetAppliedMigrationsAsync()).Count();
    }

    public async Task ApplyMigrationsAsync()
    {
        await _context.Database.MigrateAsync();
    }
}
