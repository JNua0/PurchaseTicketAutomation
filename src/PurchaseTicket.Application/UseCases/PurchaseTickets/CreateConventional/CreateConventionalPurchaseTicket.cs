using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Abstractions.TicketNumbers;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Results;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.CreateConventional;

public class CreateConventionalPurchaseTicket
{
    private readonly IPurchaseTicketRepository _repository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMaterialRepository _materialRepository;
    private readonly ITicketNumberGenerator _ticketNumberGenerator;

    public CreateConventionalPurchaseTicket(
        IPurchaseTicketRepository repository,
        ISupplierRepository supplierRepository,
        IMaterialRepository materialRepository,
        ITicketNumberGenerator ticketNumberGenerator)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _materialRepository = materialRepository;
        _ticketNumberGenerator = ticketNumberGenerator;
    }

    public async Task<CreatePurchaseTicketResult> ExecuteAsync(
        CreateConventionalPurchaseTicketCommand command)
    {
        var supplier =
            await _supplierRepository.GetByIdAsync(
                command.SupplierId);

        if (supplier is null)
            throw new InvalidOperationException(
                "Supplier was not found.");

        var material =
            await _materialRepository.GetByIdAsync(
                command.MaterialId);

        if (material is null)
            throw new InvalidOperationException(
                "Material was not found.");

        string ticketNumber =
            await _ticketNumberGenerator.GenerateAsync();

        var ticket = Ticket.CreateConventional(
            ticketNumber,
            command.SupplierId,
            command.MaterialId,
            command.LicensePlate,
            command.Transporter,
            command.GrossWeight);

        await _repository.AddAsync(ticket);

        return new CreatePurchaseTicketResult(
            ticket.TicketNumber);
    }
}