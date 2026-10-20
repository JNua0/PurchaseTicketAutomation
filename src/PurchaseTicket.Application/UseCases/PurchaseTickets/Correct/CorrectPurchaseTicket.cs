using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Domain.ValueObjects;

namespace PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;

public class CorrectPurchaseTicket
{
    private readonly IPurchaseTicketRepository _repository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMaterialRepository _materialRepository;

    public CorrectPurchaseTicket(
        IPurchaseTicketRepository repository,
        ISupplierRepository supplierRepository,
        IMaterialRepository materialRepository)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _materialRepository = materialRepository;
    }

    public async Task<CorrectPurchaseTicketResult> ExecuteAsync(
        CorrectPurchaseTicketCommand command)
    {
        var ticket = await _repository.GetByIdAsync(command.TicketId);

        if (ticket is null)
            throw new InvalidOperationException("No se encontró el ticket.");

        bool hasCorrections =
            command.SupplierId.HasValue ||
            command.MaterialId.HasValue ||
            command.Transporter is not null ||
            command.ChangeLicensePlate ||
            command.GrossWeight.HasValue ||
            command.TareWeight.HasValue ||
            command.NetWeight.HasValue ||
            command.Discount.HasValue ||
            command.PricePerKg.HasValue;

        if (!hasCorrections)
            throw new InvalidOperationException("No se indicó ningún cambio para corregir.");

        // 1. Validate external references

        if (command.SupplierId.HasValue)
        {
            var supplier = await _supplierRepository.GetByIdAsync(command.SupplierId.Value);

            if (supplier is null)
                throw new InvalidOperationException("No se encontró el proveedor.");

            if (!supplier.IsActive)
                throw new InvalidOperationException("El proveedor seleccionado está inactivo.");
        }

        if (command.MaterialId.HasValue)
        {
            var material = await _materialRepository.GetByIdAsync(command.MaterialId.Value);

            if (material is null)
                throw new InvalidOperationException("No se encontró el material.");

            if (!material.IsActive)
                throw new InvalidOperationException("El material seleccionado está inactivo.");
        }

        // 2. Apply correction

        var correction =
            new PurchaseTicketCorrection(
                SupplierId: command.SupplierId,
                MaterialId: command.MaterialId,
                Transporter: command.Transporter,
                ChangeLicensePlate: command.ChangeLicensePlate,
                LicensePlate: command.LicensePlate,
                GrossWeight: command.GrossWeight,
                TareWeight: command.TareWeight,
                NetWeight: command.NetWeight,
                Discount: command.Discount,
                PricePerKg: command.PricePerKg);

        ticket.Correct(correction);

        // 3. Persist once

        await _repository.UpdateAsync(ticket);

        return new CorrectPurchaseTicketResult(ticket.TicketNumber);
    }
}