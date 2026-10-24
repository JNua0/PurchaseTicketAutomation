using PurchaseTicket.Application.Abstractions.Persistence.Models;
using PurchaseTicket.Application.UseCases.Reports.Purchases;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Application.Tests.UseCases.Reports;

public class PurchaseReportTests
{
    private static readonly DateTime From = new(2026, 10, 1, 6, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new(2026, 10, 24, 6, 0, 0, DateTimeKind.Utc);

    // Completado: 10 % de descuento a $2 por kg
    private static ReportTicket Completed(
        string number,
        int supplierId,
        string supplier,
        int materialId,
        string material,
        decimal netWeight,
        bool isSupplierActive = true)
    {
        decimal afterDiscount = netWeight * 0.9m;

        return new ReportTicket(
            number, From.AddHours(1), supplierId, supplier, isSupplierActive, materialId, material, true,
            "ABC123", "Juan Perez", WeighingType.Conventional, TicketStatus.Completed,
            netWeight, 10, afterDiscount, 2m, afterDiscount * 2m);
    }

    private static ReportTicket Pending(
        string number,
        int supplierId,
        string supplier,
        int materialId,
        string material,
        decimal netWeight)
    {
        return new ReportTicket(
            number, From.AddHours(2), supplierId, supplier, true, materialId, material, true,
            null, "Juan Perez", WeighingType.Single, TicketStatus.AmountPending,
            netWeight, null, null, null, null);
    }

    [Fact]
    public void Create_ShouldSumTotalsWithAmountOnlyFromCompleted()
    {
        // Arrange
        ReportTicket[] tickets =
        [
            Completed("1", 1, "Proveedor Uno", 1, "Carton", 1000m),
            Completed("2", 2, "Proveedor Dos", 1, "Carton", 500m),
            Pending("3", 1, "Proveedor Uno", 2, "Archivo", 300m)
        ];

        // Act
        var report = PurchaseReport.Create(From, To, tickets);

        // Assert
        Assert.Equal(3, report.TicketCount);
        Assert.Equal(2, report.CompletedCount);
        Assert.Equal(1, report.PendingAmountCount);
        Assert.Equal(1800m, report.NetWeight);
        Assert.Equal(1350m, report.NetWeightAfterDiscount);
        Assert.Equal(2700m, report.Amount);
    }

    [Fact]
    public void Create_ShouldGroupByMaterialAndSupplierOrderedByWeight()
    {
        // Arrange
        ReportTicket[] tickets =
        [
            Completed("1", 1, "Proveedor Uno", 1, "Carton", 1000m),
            Completed("2", 2, "Proveedor Dos", 1, "Carton", 500m),
            Pending("3", 1, "Proveedor Uno", 2, "Archivo", 300m),
            Completed("4", 3, "Proveedor Tres", 2, "Archivo", 2000m)
        ];

        // Act
        var report = PurchaseReport.Create(From, To, tickets);

        // Assert
        Assert.Collection(
            report.ByMaterial,
            archivo =>
            {
                Assert.Equal("Archivo", archivo.Name);
                Assert.Equal(2, archivo.TicketCount);
                Assert.Equal(1, archivo.PendingAmountCount);
                Assert.Equal(2300m, archivo.NetWeight);
                Assert.Equal(3600m, archivo.Amount);
            },
            carton =>
            {
                Assert.Equal("Carton", carton.Name);
                Assert.Equal(1500m, carton.NetWeight);
                Assert.Equal(0, carton.PendingAmountCount);
            });

        Assert.Equal(
            new[] { "Proveedor Tres", "Proveedor Uno", "Proveedor Dos" },
            report.BySupplier.Select(group => group.Name).ToArray());
    }

    [Fact]
    public void Create_ShouldBreakWeightTiesByName()
    {
        // Arrange
        ReportTicket[] tickets =
        [
            Completed("1", 2, "Zapata", 1, "Carton", 100m),
            Completed("2", 1, "Álvarez", 1, "Carton", 100m)
        ];

        // Act
        var report = PurchaseReport.Create(From, To, tickets);

        // Assert
        Assert.Equal(
            new[] { "Álvarez", "Zapata" },
            report.BySupplier.Select(group => group.Name).ToArray());
    }

    [Fact]
    public void Create_ShouldKeepInactiveSuppliersThatHaveTickets()
    {
        // Arrange
        ReportTicket[] tickets =
        [
            Completed("1", 1, "Proveedor Viejo", 1, "Carton", 100m, isSupplierActive: false)
        ];

        // Act
        var report = PurchaseReport.Create(From, To, tickets);

        // Assert
        var supplier = Assert.Single(report.BySupplier);
        Assert.False(supplier.IsActive);
    }

    [Fact]
    public void ShareOf_ShouldBeThePartOfTheTotalWeight()
    {
        // Arrange
        ReportTicket[] tickets =
        [
            Completed("1", 1, "Proveedor Uno", 1, "Carton", 750m),
            Completed("2", 2, "Proveedor Dos", 2, "Archivo", 250m)
        ];

        var report = PurchaseReport.Create(From, To, tickets);

        // Act / Assert
        Assert.Equal(0.75m, report.ShareOf(report.ByMaterial[0]));
        Assert.Equal(0.25m, report.ShareOf(report.ByMaterial[1]));
    }

    [Fact]
    public void Create_ShouldBeEmptyWithoutTickets()
    {
        // Act
        var report = PurchaseReport.Create(From, To, []);

        // Assert
        Assert.True(report.IsEmpty);
        Assert.Empty(report.ByMaterial);
        Assert.Empty(report.BySupplier);
        Assert.Equal(0m, report.Amount);
    }
}
