using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.Abstractions.Printing;

public sealed record TicketPrintData(
    Ticket Ticket,
    string SupplierName,
    string MaterialName);