using System.Globalization;
using System.Text;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Infrastructure.Printing.Serial;

namespace PurchaseTicket.Infrastructure.Printing;

public sealed class EpsonTmU295TicketPrinter : ITicketPrinter
{
    private const int LineWidth = 35;

    private readonly ISerialPortFactory _serialPortFactory;
    private readonly TicketPrinterOptions _options;

    public EpsonTmU295TicketPrinter(
        ISerialPortFactory serialPortFactory,
        TicketPrinterOptions options)
    {
        _serialPortFactory = serialPortFactory;
        _options = options;
    }

    public Task PrintInitialAsync(TicketPrintData data)
    {
        using var serialPort = _serialPortFactory.Create();

        try
        {
            serialPort.Open();

            string text = BuildInitialTicket(data);

            serialPort.Write(text);

            return Task.CompletedTask;
        }
        finally
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }
    }

    public Task PrintFinalAsync(TicketPrintData data)
    {
        using var serialPort =
            _serialPortFactory.Create();

        try
        {
            serialPort.Open();

            string content =
                BuildFinalTicket(data);

            serialPort.Write(content);

            return Task.CompletedTask;
        }
        finally
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }
    }

    private string BuildInitialTicket(
        TicketPrintData data)
    {
        var ticket = data.Ticket;

        var builder = new StringBuilder();

        builder.AppendLine(_options.CompanyName);
        builder.AppendLine(_options.Address);
        builder.AppendLine(_options.Neighborhood);
        builder.AppendLine(
            $"TEL. {_options.PhoneNumber}");
        builder.AppendLine(_options.Email);

        builder.AppendLine(
            new string('=', LineWidth));

        AppendField(
            builder,
            "FOLIO",
            ticket.TicketNumber);

        AppendField(
            builder,
            "PLACAS",
            ticket.LicensePlate);

        AppendField(
            builder,
            "PROVEEDOR",
            data.SupplierName);

        AppendField(
            builder,
            "CHOFER",
            ticket.DriverName);

        AppendField(
            builder,
            "PRODUCTO",
            data.MaterialName);

        builder.AppendLine(
            new string('-', LineWidth));

        builder.AppendLine("ENTRADA");

        AppendField(
            builder,
            "FECHA",
            ticket.CreatedAt.ToString(
                "dd/MM/yyyy HH:mm"));

        AppendField(
            builder,
            "PESO",
            $"{FormatWeight(ticket.GrossWeight)} kg");

        return builder.ToString()
            .Replace(
                Environment.NewLine,
                "\r\n");
    }

    private static string BuildFinalTicket(
        TicketPrintData data)
    {
        var ticket = data.Ticket;

        if (ticket.CompletedAt is null)
            throw new InvalidOperationException(
                "Purchase ticket has not been completed.");

        if (ticket.TareWeight is null)
            throw new InvalidOperationException(
                "Purchase ticket does not have a tare weight.");

        if (ticket.NetWeight is null)
            throw new InvalidOperationException(
                "Purchase ticket does not have a net weight.");

        var builder = new StringBuilder();

        builder.AppendLine(
            new string('-', LineWidth));

        builder.AppendLine("SALIDA");

        AppendField(
            builder,
            "FECHA",
            ticket.CompletedAt.Value.ToString(
                "dd/MM/yyyy HH:mm"));

        AppendField(
            builder,
            "PESO",
            $"{FormatWeight(ticket.TareWeight.Value)} kg");

        builder.AppendLine(
            new string('-', LineWidth));

        AppendField(
            builder,
            "PESO NETO",
            $"{FormatWeight(ticket.NetWeight.Value)} kg");

        builder.AppendLine(
            new string('=', LineWidth));

        return builder.ToString()
            .Replace(
                Environment.NewLine,
                "\r\n");
    }

    private static string FormatWeight(decimal weight)
    {
        return weight.ToString(
            "#,##0",
            CultureInfo.InvariantCulture);
    }

    private static void AppendField(
        StringBuilder builder,
        string label,
        string value)
    {
        string prefix = $"{label.PadRight(9)}: ";
        string indentation = new(' ', prefix.Length);

        var words = value.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

        var currentLine = new StringBuilder(prefix);

        foreach (var word in words)
        {
            bool isFirstWord =
                currentLine.Length == prefix.Length;

            int requiredLength =
                word.Length + (isFirstWord ? 0 : 1);

            if (currentLine.Length + requiredLength
                <= LineWidth)
            {
                if (!isFirstWord)
                    currentLine.Append(' ');

                currentLine.Append(word);
            }
            else
            {
                builder.AppendLine(
                    currentLine.ToString());

                currentLine.Clear();
                currentLine.Append(indentation);
                currentLine.Append(word);
            }
        }

        builder.AppendLine(
            currentLine.ToString());
    }


}