using PurchaseTicket.Application.Abstractions.Persistence;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Domain.Exceptions;
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
            throw new NotFoundException("No se encontró el ticket.");

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
            throw new BusinessRuleException("No se indicó ningún cambio para corregir.");

        // El motivo se valida antes de tocar el ticket
        string? reason = TicketCorrection.NormalizeReason(command.Reason);

        // 1. Validate external references

        // Nombres de proveedor y material para el historial: el anterior y el nuevo
        string? previousSupplierName = null;
        string? newSupplierName = null;
        string? previousMaterialName = null;
        string? newMaterialName = null;

        if (command.SupplierId.HasValue)
        {
            var supplier = await _supplierRepository.GetByIdAsync(command.SupplierId.Value);

            if (supplier is null)
                throw new NotFoundException("No se encontró el proveedor.");

            if (!supplier.IsActive)
                throw new BusinessRuleException("El proveedor seleccionado está inactivo.");

            newSupplierName = supplier.Name;
            previousSupplierName = (await _supplierRepository.GetByIdAsync(ticket.SupplierId))?.Name;
        }

        if (command.MaterialId.HasValue)
        {
            var material = await _materialRepository.GetByIdAsync(command.MaterialId.Value);

            if (material is null)
                throw new NotFoundException("No se encontró el material.");

            if (!material.IsActive)
                throw new BusinessRuleException("El material seleccionado está inactivo.");

            newMaterialName = material.Name;
            previousMaterialName = (await _materialRepository.GetByIdAsync(ticket.MaterialId))?.Name;
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

        var changes = ticket.Correct(correction);

        // 3. Record history: proveedor y material por nombre, para que se lea
        // igual aunque despues se renombren. Sin cambios reales no hay registro

        if (changes.Count > 0)
        {
            var namedChanges = changes
                .Select(change => change.Field switch
                {
                    TicketField.Supplier => change with
                    {
                        Before = previousSupplierName ?? change.Before,
                        After = newSupplierName ?? change.After
                    },
                    TicketField.Material => change with
                    {
                        Before = previousMaterialName ?? change.Before,
                        After = newMaterialName ?? change.After
                    },
                    _ => change
                });

            ticket.RecordCorrection(reason, namedChanges);
        }

        // 4. Persist once: el ticket y su correccion

        await _repository.UpdateAsync(ticket);

        return new CorrectPurchaseTicketResult(ticket.TicketNumber);
    }
}