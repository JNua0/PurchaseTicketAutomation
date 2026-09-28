using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PurchaseTicket.Application.Abstractions.TicketNumbers;
using PurchaseTicket.Infrastructure.Persistence;

namespace PurchaseTicket.Infrastructure.TicketNumbers;

public class TicketNumberGenerator : ITicketNumberGenerator
{
    private readonly ApplicationDbContext _context;

    public TicketNumberGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync()
    {
        var connection = _context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();

        command.CommandText =
            "SELECT nextval('purchase_ticket_number_seq');";

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt64(result)
            .ToString(CultureInfo.InvariantCulture);
    }
}