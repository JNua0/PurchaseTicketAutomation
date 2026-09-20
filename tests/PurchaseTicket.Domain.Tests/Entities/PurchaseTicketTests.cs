using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Domain.Tests.Entities;

public class PurchaseTicketTests
{
    [Fact]
    public void Constructor_ShouldCreateTicketWithPendingStatus()
    {
        // Arrange & Act
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Assert
        Assert.Equal(TicketStatus.Pending, ticket.Status);
    }

    [Fact]
    public void Constructor_ShouldCreateTicketWithProvidedData()
    {
        // Arrange → preparar los datos necesarios
        const string ticketNumber = "T-000001";
        const int supplierCustomerId = 1;
        const int materialId = 2;
        const string licensePlate = "ABC123";
        const string driverName = "Juan Perez";
        const decimal grossWeight = 25000m;

        // Act → ejecutar el comportamiento
        var ticket = new Ticket(
            ticketNumber,
            supplierCustomerId,
            materialId,
            licensePlate,
            driverName,
            grossWeight
        );

        // Assert → comprobar el resultado
        Assert.Equal(ticketNumber, ticket.TicketNumber);
        Assert.Equal(supplierCustomerId, ticket.SupplierCustomerId);
        Assert.Equal(materialId, ticket.MaterialId);
        Assert.Equal(licensePlate, ticket.LicensePlate);
        Assert.Equal(driverName, ticket.DriverName);
        Assert.Equal(grossWeight, ticket.GrossWeight);
    }

    [Fact]
    public void Constructor_ShouldNormalizeLicensePlateAndDriverName()
    {
        // Arrange
        const string licensePlate = "  abc123  ";
        const string driverName = "  jUAN   peREZ  ";

        // Act
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            licensePlate,
            driverName,
            25000m
        );

        // Assert
        Assert.Equal("ABC123", ticket.LicensePlate);
        Assert.Equal("Juan Perez", ticket.DriverName);
    }

    [Fact]
    public void Constructor_ShouldInitializeCompletionDataAsNull()
    {
        // Arrange & Act
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Assert
        Assert.Null(ticket.TareWeight);
        Assert.Null(ticket.NetWeight);
        Assert.Null(ticket.Discount);
        Assert.Null(ticket.DiscountWeight);
        Assert.Null(ticket.NetWeightAfterDiscount);
        Assert.Null(ticket.PricePerKg);
        Assert.Null(ticket.Amount);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenTicketNumberIsEmpty()
    {
        // Arrange
        const string ticketNumber = "";

        // Act
        Action act = () => new Ticket(
            ticketNumber,
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Constructor_ShouldThrowWhenTicketNumberIsEmptyOrWhitespace(
    string ticketNumber)
    {
        // Act
        Action act = () => new Ticket(
            ticketNumber,
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldThrowWhenSupplierCustomerIdIsInvalid(
    int supplierCustomerId)
    {
        // Act
        Action act = () => new Ticket(
            "T-000001",
            supplierCustomerId,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldThrowWhenMaterialIdIsInvalid(
    int materialId)
    {
        // Act
        Action act = () => new Ticket(
            "T-000001",
            1,
            materialId,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1000)]
    public void Constructor_ShouldThrowWhenGrossWeightIsNotPositive(
    decimal grossWeight)
    {
        // Act
        Action act = () => new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            grossWeight
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Constructor_ShouldThrowWhenLicensePlateIsEmptyOrWhitespace(
    string licensePlate)
    {
        // Act
        Action act = () => new Ticket(
            "T-000001",
            1,
            1,
            licensePlate,
            "Juan Perez",
            25000m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Constructor_ShouldThrowWhenDriverNameIsEmptyOrWhitespace(
    string driverName)
    {
        // Act
        Action act = () => new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            driverName,
            25000m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenTicketNumberExceedsMaximumLength()
    {
        // Arrange
        string ticketNumber = new('A', 21);

        // Act
        Action act = () => new Ticket(
            ticketNumber,
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenLicensePlateExceedsMaximumLength()
    {
        // Arrange
        string licensePlate = new('A', 11);

        // Act
        Action act = () => new Ticket(
            "T-000001",
            1,
            1,
            licensePlate,
            "Juan Perez",
            25000m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenDriverNameExceedsMaximumLength()
    {
        // Arrange
        string driverName = new('A', 41);

        // Act
        Action act = () => new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            driverName,
            25000m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Complete_ShouldCalculateValuesAndSetStatusToCompleted()
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        const decimal tareWeight = 10000m;
        const decimal discount = 5m;
        const decimal pricePerKg = 8.50m;

        // Act
        ticket.Complete(tareWeight, discount, pricePerKg);

        // Assert
        Assert.Equal(tareWeight, ticket.TareWeight);
        Assert.Equal(15000m, ticket.NetWeight);
        Assert.Equal(discount, ticket.Discount);
        Assert.Equal(750m, ticket.DiscountWeight);
        Assert.Equal(14250m, ticket.NetWeightAfterDiscount);
        Assert.Equal(pricePerKg, ticket.PricePerKg);
        Assert.Equal(121125m, ticket.Amount);
        Assert.Equal(TicketStatus.Completed, ticket.Status);
    }

    [Fact]
    public void Complete_ShouldThrowWhenTicketIsCancelled()
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        ticket.Cancel();

        // Act
        Action act = () => ticket.Complete(
            10000m,
            5m,
            8.50m
        );

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Complete_ShouldThrowWhenTicketIsAlreadyCompleted()
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        ticket.Complete(
            10000m,
            5m,
            8.50m
        );

        // Act
        Action act = () => ticket.Complete(
            9000m,
            10m,
            9m
        );

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1000)]
    public void Complete_ShouldThrowWhenTareWeightIsNotPositive(
    decimal tareWeight)
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Act
        Action act = () => ticket.Complete(
            tareWeight,
            5m,
            8.50m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(25000)]
    [InlineData(26000)]
    public void Complete_ShouldThrowWhenTareWeightIsGreaterThanOrEqualToGrossWeight(
    decimal tareWeight)
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Act
        Action act = () => ticket.Complete(
            tareWeight,
            5m,
            8.50m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Complete_ShouldThrowWhenDiscountIsOutsideValidRange(
    decimal discount)
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Act
        Action act = () => ticket.Complete(
            10000m,
            discount,
            8.50m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Complete_ShouldThrowWhenPricePerKgIsNotPositive(
    decimal pricePerKg)
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Act
        Action act = () => ticket.Complete(
            10000m,
            5m,
            pricePerKg
        );

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled()
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Act
        ticket.Cancel();

        // Assert
        Assert.Equal(TicketStatus.Cancelled, ticket.Status);
    }

    [Fact]
    public void Cancel_ShouldThrowWhenTicketIsCompleted()
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        ticket.Complete(
            10000m,
            5m,
            8.50m
        );

        // Act
        Action act = () => ticket.Cancel();

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Cancel_ShouldThrowWhenTicketIsAlreadyCancelled()
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        ticket.Cancel();

        // Act
        Action act = () => ticket.Cancel();

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Correct_ShouldUpdateInputDataWhenTicketIsPending()
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Act
        ticket.Correct(
            2,
            3,
            "XYZ789",
            "Pedro Lopez",
            30000m
        );

        // Assert
        Assert.Equal(2, ticket.SupplierCustomerId);
        Assert.Equal(3, ticket.MaterialId);
        Assert.Equal("XYZ789", ticket.LicensePlate);
        Assert.Equal("Pedro Lopez", ticket.DriverName);
        Assert.Equal(30000m, ticket.GrossWeight);
    }

    [Fact]
    public void Correct_ShouldThrowWhenTicketIsCompleted()
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        ticket.Complete(
            10000m,
            5m,
            8.50m
        );

        // Act
        Action act = () => ticket.Correct(
            2,
            2,
            "XYZ789",
            "Pedro Lopez",
            30000m
        );

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Correct_ShouldThrowWhenTicketIsCancelled()
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        ticket.Cancel();

        // Act
        Action act = () => ticket.Correct(
            2,
            2,
            "XYZ789",
            "Pedro Lopez",
            30000m
        );

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Correct_ShouldNormalizeLicensePlateAndDriverName()
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Act
        ticket.Correct(
            2,
            2,
            "  xyz789  ",
            "  peDRO   loPEZ  ",
            30000m
        );

        // Assert
        Assert.Equal("XYZ789", ticket.LicensePlate);
        Assert.Equal("Pedro Lopez", ticket.DriverName);
    }

    [Fact]
    public void Correct_ShouldNotModifyTicketWhenValidationFails()
    {
        // Arrange
        var ticket = new Ticket(
            "T-000001",
            1,
            1,
            "ABC123",
            "Juan Perez",
            25000m
        );

        // Act
        Action act = () => ticket.Correct(
            2,
            3,
            "XYZ789",
            "Pedro Lopez",
            -100m
        );

        // Assert
        Assert.Throws<ArgumentException>(act);

        Assert.Equal(1, ticket.SupplierCustomerId);
        Assert.Equal(1, ticket.MaterialId);
        Assert.Equal("ABC123", ticket.LicensePlate);
        Assert.Equal("Juan Perez", ticket.DriverName);
        Assert.Equal(25000m, ticket.GrossWeight);
        Assert.Equal(TicketStatus.Pending, ticket.Status);
    }


}