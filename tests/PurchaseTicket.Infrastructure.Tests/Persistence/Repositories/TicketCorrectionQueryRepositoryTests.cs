using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Domain.ValueObjects;
using PurchaseTicket.Infrastructure.Persistence.Repositories;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Infrastructure.Tests.Persistence.Repositories;

public class TicketCorrectionQueryRepositoryTests : InfrastructureTestBase
{
    // Un completado: 25,000 - 8,000 kg, 10 % de descuento, $2 por kg
    // Proveedor y material propios: sus nombres no se repiten
    private async Task<Ticket> SeedCompletedTicketAsync(
        string ticketNumber = "T-0001",
        string supplierName = "Proveedor Uno",
        string materialName = "Acero")
    {
        var supplier = new Supplier(supplierName);
        var material = new Material(materialName);

        Context.Suppliers.Add(supplier);
        Context.Materials.Add(material);

        await Context.SaveChangesAsync();

        var ticket =
            Ticket.CreateConventional(
                ticketNumber: ticketNumber,
                supplierId: supplier.Id,
                materialId: material.Id,
                licensePlate: "ABC123",
                transporter: "Juan Perez",
                grossWeight: 25000m);

        ticket.RegisterTare(8000m);
        ticket.RegisterAmount(discount: 10, pricePerKg: 2m);

        Context.PurchaseTickets.Add(ticket);

        await Context.SaveChangesAsync();

        return ticket;
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistCorrectionWithChanges()
    {
        // Arrange
        var ticket = await SeedCompletedTicketAsync();

        var changes = ticket.Correct(
            new PurchaseTicketCorrection(TareWeight: 8100m));

        ticket.RecordCorrection("Tara mal capturada", changes);

        // Act
        await new PurchaseTicketRepository(Context).UpdateAsync(ticket);

        Context.ChangeTracker.Clear();

        var corrections =
            await new TicketCorrectionQueryRepository(Context)
                .GetByTicketIdAsync(ticket.Id);

        // Assert
        var correction = Assert.Single(corrections);

        Assert.Equal("Tara mal capturada", correction.Reason);
        Assert.Equal(DateTimeKind.Utc, correction.CorrectedAt.Kind);

        Assert.Collection(
            correction.Changes,
            tare =>
            {
                Assert.Equal(TicketField.TareWeight, tare.Field);
                Assert.False(tare.IsRecalculated);
            },
            netWeight =>
            {
                Assert.Equal(TicketField.NetWeight, netWeight.Field);
                Assert.True(netWeight.IsRecalculated);
            },
            amount =>
            {
                Assert.Equal(TicketField.Amount, amount.Field);
                Assert.True(amount.IsRecalculated);
            });
    }

    [Fact]
    public async Task GetByTicketIdAsync_ShouldReturnNewestFirstAndOnlyThatTicket()
    {
        // Arrange
        var ticket = await SeedCompletedTicketAsync("T-0001");
        var otherTicket = await SeedCompletedTicketAsync("T-0002", "Proveedor Dos", "Cobre");

        var repository = new PurchaseTicketRepository(Context);

        ticket.RecordCorrection(
            "Primera",
            ticket.Correct(new PurchaseTicketCorrection(PricePerKg: 2.5m)));

        await repository.UpdateAsync(ticket);

        ticket.RecordCorrection(
            "Segunda",
            ticket.Correct(new PurchaseTicketCorrection(Discount: 5)));

        await repository.UpdateAsync(ticket);

        otherTicket.RecordCorrection(
            "De otro ticket",
            otherTicket.Correct(new PurchaseTicketCorrection(TareWeight: 7000m)));

        await repository.UpdateAsync(otherTicket);

        Context.ChangeTracker.Clear();

        // Act
        var corrections =
            await new TicketCorrectionQueryRepository(Context)
                .GetByTicketIdAsync(ticket.Id);

        // Assert
        Assert.Equal(
            new[] { "Segunda", "Primera" },
            corrections.Select(correction => correction.Reason).ToArray());
    }

    [Fact]
    public async Task GetByTicketIdAsync_ShouldReturnEmptyWhenTicketHasNoCorrections()
    {
        // Arrange
        var ticket = await SeedCompletedTicketAsync();

        // Act
        var corrections =
            await new TicketCorrectionQueryRepository(Context)
                .GetByTicketIdAsync(ticket.Id);

        // Assert
        Assert.Empty(corrections);
    }
}
