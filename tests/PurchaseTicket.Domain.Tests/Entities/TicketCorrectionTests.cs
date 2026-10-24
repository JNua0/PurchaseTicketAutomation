using System.Globalization;
using PurchaseTicket.Domain.Entities;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Domain.ValueObjects;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Domain.Tests.Entities;

public class TicketCorrectionTests
{
    // 25,000 - 8,000 = 17,000 kg; 10 % de descuento = 15,300 kg; a $2 = $30,600
    private static Ticket CreateCompletedTicket()
    {
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(8000m);
        ticket.RegisterAmount(discount: 10, pricePerKg: 2m);

        return ticket;
    }

    private static decimal ParseInvariant(string? value)
    {
        return decimal.Parse(value!, CultureInfo.InvariantCulture);
    }

    [Fact]
    public void Correct_ShouldReturnTareChangeAndRecalculatedValues()
    {
        // Arrange
        var ticket = CreateCompletedTicket();

        // Act
        var changes = ticket.Correct(
            new PurchaseTicketCorrection(TareWeight: 8100m));

        // Assert
        Assert.Equal(
            new[] { TicketField.TareWeight, TicketField.NetWeight, TicketField.Amount },
            changes.Select(change => change.Field));

        var tare = changes[0];
        Assert.Equal(8000m, ParseInvariant(tare.Before));
        Assert.Equal(8100m, ParseInvariant(tare.After));
        Assert.False(tare.IsRecalculated);

        var netWeight = changes[1];
        Assert.Equal(17000m, ParseInvariant(netWeight.Before));
        Assert.Equal(16900m, ParseInvariant(netWeight.After));
        Assert.True(netWeight.IsRecalculated);

        var amount = changes[2];
        Assert.Equal(30600m, ParseInvariant(amount.Before));
        Assert.Equal(30420m, ParseInvariant(amount.After));
        Assert.True(amount.IsRecalculated);
    }

    [Fact]
    public void Correct_ShouldReturnNoChangesWhenValuesAreEqual()
    {
        // Arrange
        var ticket = CreateCompletedTicket();

        // Act
        var changes = ticket.Correct(
            new PurchaseTicketCorrection(TareWeight: 8000m, Discount: 10));

        // Assert
        Assert.Empty(changes);
    }

    [Fact]
    public void Correct_ShouldReturnSupplierAndLicensePlateChanges()
    {
        // Arrange
        var ticket = CreateCompletedTicket();

        // Act
        var changes = ticket.Correct(
            new PurchaseTicketCorrection(
                SupplierId: 2,
                ChangeLicensePlate: true,
                LicensePlate: "xyz789"));

        // Assert
        Assert.Collection(
            changes,
            supplier =>
            {
                Assert.Equal(TicketField.Supplier, supplier.Field);
                Assert.Equal("1", supplier.Before);
                Assert.Equal("2", supplier.After);
            },
            licensePlate =>
            {
                Assert.Equal(TicketField.LicensePlate, licensePlate.Field);
                Assert.Equal("ABC123", licensePlate.Before);
                Assert.Equal("XYZ789", licensePlate.After);
            });
    }

    [Fact]
    public void Correct_ShouldNotMarkNetWeightAsRecalculatedOnSingleWeighing()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            12000m);

        // Act
        var changes = ticket.Correct(
            new PurchaseTicketCorrection(NetWeight: 12500m));

        // Assert
        var netWeight = Assert.Single(changes);
        Assert.Equal(TicketField.NetWeight, netWeight.Field);
        Assert.False(netWeight.IsRecalculated);
    }

    [Fact]
    public void RecordCorrection_ShouldAddCorrectionWithTrimmedReason()
    {
        // Arrange
        var ticket = CreateCompletedTicket();

        var changes = ticket.Correct(
            new PurchaseTicketCorrection(TareWeight: 8100m));

        // Act
        var correction = ticket.RecordCorrection("  Tara mal capturada  ", changes);

        // Assert
        Assert.Same(correction, Assert.Single(ticket.Corrections));
        Assert.Equal("Tara mal capturada", correction.Reason);
        Assert.Equal(3, correction.Changes.Count);
        Assert.Equal(DateTimeKind.Utc, correction.CorrectedAt.Kind);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RecordCorrection_ShouldStoreNullReasonWhenBlank(string? reason)
    {
        // Arrange
        var ticket = CreateCompletedTicket();

        var changes = ticket.Correct(
            new PurchaseTicketCorrection(PricePerKg: 2.5m));

        // Act
        var correction = ticket.RecordCorrection(reason, changes);

        // Assert
        Assert.Null(correction.Reason);
    }

    [Fact]
    public void RecordCorrection_ShouldThrowWhenReasonIsTooLong()
    {
        // Arrange
        var ticket = CreateCompletedTicket();

        var changes = ticket.Correct(
            new PurchaseTicketCorrection(PricePerKg: 2.5m));

        string reason = new('a', TicketCorrection.ReasonMaxLength + 1);

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => ticket.RecordCorrection(reason, changes));

        // Assert
        Assert.Equal(
            "El motivo no puede exceder los 200 caracteres.",
            exception.Message);

        Assert.Empty(ticket.Corrections);
    }

    [Fact]
    public void RecordCorrection_ShouldThrowWhenThereAreNoChanges()
    {
        // Arrange
        var ticket = CreateCompletedTicket();

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => ticket.RecordCorrection("Sin cambios", []));

        // Assert
        Assert.Equal(
            "Una corrección debe tener al menos un cambio.",
            exception.Message);
    }
}
