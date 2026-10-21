using System.Globalization;
using System.Text;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Infrastructure.Printing.Serial;

namespace PurchaseTicket.Infrastructure.Printing;

public sealed class EpsonTmU295TicketPrinter : ITicketPrinter
{
    // Campos

    private readonly ISerialPortFactory _serialPortFactory;
    private readonly TicketPrinterOptions _options;
    private readonly TimeProvider _timeProvider;

    // Constructor

    public EpsonTmU295TicketPrinter(
        ISerialPortFactory serialPortFactory,
        TicketPrinterOptions options,
        TimeProvider timeProvider)
    {
        _serialPortFactory = serialPortFactory;
        _options = options;
        _timeProvider = timeProvider;
    }

    // Metodos publicos

    public Task PrintConventionalInitialAsync(TicketPrintData data)
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

    public Task PrintConventionalFinalAsync(TicketPrintData data)
    {
        using var serialPort = _serialPortFactory.Create();

        try
        {
            serialPort.Open();

            string content = BuildFinalTicket(data);

            serialPort.Write(content);

            return Task.CompletedTask;
        }
        finally
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }
    }

    public Task PrintConventionalCompletedAsync(TicketPrintData data)
    {
        using var serialPort = _serialPortFactory.Create();

        try
        {
            serialPort.Open();

            string content =
                BuildInitialTicket(data) +
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

    public Task PrintSingleAsync(TicketPrintData data)
    {
        using var serialPort = _serialPortFactory.Create();

        try
        {
            serialPort.Open();

            string content =
                BuildSingleTicket(data);

            serialPort.Write(content);

            return Task.CompletedTask;
        }
        finally
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }
    }

    // Metodos privados

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
                <= TicketFormat.LineWidth)
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

    private static void AppendHeaderLine(
        StringBuilder builder,
        string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            builder.AppendLine(value.Trim());
    }

    // Los campos vacios del encabezado no se imprimen: un campo opcional sin
    // valor dejaria un renglon en blanco o un "TEL." sin numero
    private void AppendHeader(StringBuilder builder)
    {
        AppendHeaderLine(builder, _options.CompanyName);
        AppendHeaderLine(builder, _options.Address);
        AppendHeaderLine(builder, _options.Neighborhood);

        if (!string.IsNullOrWhiteSpace(_options.PhoneNumber))
            builder.AppendLine($"TEL. {_options.PhoneNumber.Trim()}");

        AppendHeaderLine(builder, _options.Email);

        builder.AppendLine(
            new string('=', TicketFormat.LineWidth));
    }

    private string BuildInitialTicket(TicketPrintData data)
    {
        var ticket = data.Ticket;

        if (ticket.GrossWeight is null)
            throw new InvalidOperationException("El ticket no tiene peso bruto.");

        var builder = new StringBuilder();

        AppendHeader(builder);

        AppendField(
            builder,
            "FOLIO",
            ticket.TicketNumber);

        AppendField(
            builder,
            "PLACAS",
            ticket.LicensePlate ?? string.Empty);

        AppendField(
            builder,
            "PROVEEDOR",
            data.SupplierName);

        AppendField(
            builder,
            "CHOFER",
            ticket.Transporter);

        AppendField(
            builder,
            "PRODUCTO",
            data.MaterialName);

        builder.AppendLine(
            new string('-', TicketFormat.LineWidth));

        builder.AppendLine("ENTRADA");

        AppendField(
            builder,
            "FECHA",
            FormatLocalDate(ticket.CheckInAt));

        AppendField(
            builder,
            "PESO",
            $"{FormatWeight(ticket.GrossWeight.Value)} kg");

        return builder.ToString()
            .Replace(
                Environment.NewLine,
                "\r\n");
    }

    private string BuildFinalTicket(TicketPrintData data)
    {
        var ticket = data.Ticket;

        if (ticket.DepartureAt is null)
            throw new InvalidOperationException("El ticket no tiene fecha de salida.");

        if (ticket.TareWeight is null)
            throw new InvalidOperationException("El ticket no tiene tara.");

        if (ticket.NetWeight is null)
            throw new InvalidOperationException("El ticket no tiene peso neto.");

        var builder = new StringBuilder();

        builder.AppendLine(
            new string('-', TicketFormat.LineWidth));

        builder.AppendLine("SALIDA");

        AppendField(
            builder,
            "FECHA",
            FormatLocalDate(ticket.DepartureAt.Value));

        AppendField(
            builder,
            "PESO",
            $"{FormatWeight(ticket.TareWeight.Value)} kg");

        builder.AppendLine(
            new string('-', TicketFormat.LineWidth));

        AppendField(
            builder,
            "PESO NETO",
            $"{FormatWeight(ticket.NetWeight.Value)} kg");

        builder.AppendLine(
            new string('=', TicketFormat.LineWidth));

        return builder.ToString()
            .Replace(
                Environment.NewLine,
                "\r\n");
    }

    private string BuildSingleTicket(TicketPrintData data)
    {
        var ticket = data.Ticket;

        if (ticket.NetWeight is null)
            throw new InvalidOperationException("El ticket no tiene peso neto.");

        var builder = new StringBuilder();

        AppendHeader(builder);

        AppendField(
            builder,
            "FOLIO",
            ticket.TicketNumber);

        AppendField(
            builder,
            "PLACAS",
            ticket.LicensePlate ?? string.Empty);

        AppendField(
            builder,
            "PROVEEDOR",
            data.SupplierName);

        AppendField(
            builder,
            "CHOFER",
            ticket.Transporter);

        AppendField(
            builder,
            "PRODUCTO",
            data.MaterialName);

        builder.AppendLine(new string('-', TicketFormat.LineWidth));

        builder.AppendLine("PESAJE UNICO");

        AppendField(
            builder,
            "FECHA",
            FormatLocalDate(ticket.CheckInAt));

        AppendField(
            builder,
            "PESO NETO",
            $"{FormatWeight(ticket.NetWeight.Value)} kg");

        return builder.ToString()
            .Replace(
                Environment.NewLine,
                "\r\n");
    }

    // Las fechas se guardan en UTC y se imprimen en la hora local del equipo
    private string FormatLocalDate(DateTime utcDate)
    {
        DateTime localDate =
            TimeZoneInfo.ConvertTimeFromUtc(
                utcDate,
                _timeProvider.LocalTimeZone);

        return localDate.ToString(
            "dd/MM/yyyy HH:mm",
            CultureInfo.InvariantCulture);
    }
}