using PurchaseTicket.Domain.Enums;
using Ticket = PurchaseTicket.Domain.Entities.PurchaseTicket;

namespace PurchaseTicket.Domain.Tests.Entities;

public class PurchaseTicketTests
{
    [Fact]
    public void CreateConventional_ShouldCreateTicket()
    {
        // Arrange
        const string ticketNumber = "T-000001";
        const int supplierId = 1;
        const int materialId = 1;
        const string licensePlate = "ABC1234";
        const string transporter = "Juan Perez";
        const decimal grossWeight = 25000m;

        // Act
        var ticket = Ticket.CreateConventional(
            ticketNumber,
            supplierId,
            materialId,
            licensePlate,
            transporter,
            grossWeight);

        // Assert
        Assert.Equal(ticketNumber, ticket.TicketNumber);
        Assert.Equal(supplierId, ticket.SupplierId);
        Assert.Equal(materialId, ticket.MaterialId);
        Assert.Equal(licensePlate, ticket.LicensePlate);
        Assert.Equal(transporter, ticket.Transporter);
        Assert.Equal(grossWeight, ticket.GrossWeight);

        Assert.Equal(
            WeighingType.Conventional,
            ticket.WeighingType);

        Assert.Equal(
            TicketStatus.WeighingPending,
            ticket.Status);
    }

    [Fact]
    public void CreateConventional_ShouldInitializePendingValuesAsNull()
    {
        // Act
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Assert
        Assert.Null(ticket.TareWeight);
        Assert.Null(ticket.NetWeight);
        Assert.Null(ticket.Discount);
        Assert.Null(ticket.DiscountWeight);
        Assert.Null(ticket.NetWeightAfterDiscount);
        Assert.Null(ticket.PricePerKg);
        Assert.Null(ticket.Amount);
        Assert.Null(ticket.DepartureAt);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void CreateConventional_ShouldSetCheckInAtAndCreatedAt()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        var after = DateTime.UtcNow;

        // Assert
        Assert.InRange(
            ticket.CheckInAt,
            before,
            after);

        Assert.InRange(
            ticket.CreatedAt,
            before,
            after);

        Assert.Equal(
            ticket.CheckInAt,
            ticket.CreatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateConventional_ShouldThrowWhenTicketNumberIsEmpty(string? ticketNumber)
    {
        // Act
        Action act = () => Ticket.CreateConventional(
            ticketNumber!,
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "El folio es obligatorio.",
            exception.Message);
    }

    [Fact]
    public void CreateConventional_ShouldThrowWhenTicketNumberExceeds20Characters()
    {
        // Arrange
        string ticketNumber = new('A', 21);

        // Act
        Action act = () => Ticket.CreateConventional(
            ticketNumber,
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "El folio no puede exceder los 20 caracteres.",
            exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateConventional_ShouldThrowWhenSupplierIdIsInvalid(int supplierId)
    {
        // Act
        Action act = () => Ticket.CreateConventional(
            "T-000001",
            supplierId,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Assert
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(act);

        Assert.Equal(
            "supplierId",
            exception.ParamName);

        Assert.Contains(
            "El identificador del proveedor debe ser mayor a cero.",
            exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateConventional_ShouldThrowWhenMaterialIdIsInvalid(int materialId)
    {
        // Act
        Action act = () => Ticket.CreateConventional(
            "T-000001",
            1,
            materialId,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Assert
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(act);

        Assert.Equal(
            "materialId",
            exception.ParamName);

        Assert.Contains(
            "El identificador del material debe ser mayor a cero.",
            exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateConventional_ShouldThrowWhenLicensePlateIsEmpty(string? licensePlate)
    {
        // Act
        Action act = () => Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            licensePlate!,
            "Juan Perez",
            25000m);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "Las placas son obligatorias.",
            exception.Message);
    }

    [Fact]
    public void CreateConventional_ShouldThrowWhenLicensePlateExceeds7Characters()
    {
        // Act
        Action act = () => Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC12345",
            "Juan Perez",
            25000m);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "Las placas no pueden exceder los 7 caracteres.",
            exception.Message);
    }

    [Theory]
    [InlineData("ABC 123")]
    [InlineData("ABC-123")]
    [InlineData("ABC.123")]
    [InlineData("ABC/123")]
    public void CreateConventional_ShouldThrowWhenLicensePlateContainsInvalidCharacters(string licensePlate)
    {
        // Act
        Action act = () => Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            licensePlate,
            "Juan Perez",
            25000m);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "Las placas solo pueden contener caracteres alfanuméricos.",
            exception.Message);
    }

    [Fact]
    public void CreateConventional_ShouldNormalizeLicensePlateToUppercase()
    {
        // Act
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "abc1234",
            "Juan Perez",
            25000m);

        // Assert
        Assert.Equal(
            "ABC1234",
            ticket.LicensePlate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateConventional_ShouldThrowWhenTransporterIsEmpty(string? transporter)
    {
        // Act
        Action act = () => Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            transporter!,
            25000m);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "El transportista es obligatorio.",
            exception.Message);
    }

    [Fact]
    public void CreateConventional_ShouldThrowWhenTransporterExceeds50Characters()
    {
        // Arrange
        string transporter = new('A', 51);

        // Act
        Action act = () => Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            transporter,
            25000m);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "El transportista no puede exceder los 50 caracteres.",
            exception.Message);
    }

    [Theory]
    [InlineData("Juan123")]
    [InlineData("Juan Pérez 2")]
    [InlineData("Juan-Pérez")]
    [InlineData("Juan.Pérez")]
    public void CreateConventional_ShouldThrowWhenTransporterContainsInvalidCharacters(string transporter)
    {
        // Act
        Action act = () => Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            transporter,
            25000m);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "El transportista solo puede contener letras.",
            exception.Message);
    }

    [Fact]
    public void CreateConventional_ShouldNormalizeTransporterToTitleCase()
    {
        // Act
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "JUAN PEREZ",
            25000m);

        // Assert
        Assert.Equal(
            "Juan Perez",
            ticket.Transporter);
    }

    [Fact]
    public void CreateConventional_ShouldNormalizeTransporterWhitespace()
    {
        // Act
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "   juan    perez   ",
            25000m);

        // Assert
        Assert.Equal(
            "Juan Perez",
            ticket.Transporter);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-25000)]
    public void CreateConventional_ShouldThrowWhenGrossWeightIsNotPositive(decimal grossWeight)
    {
        // Act
        Action act = () => Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            grossWeight);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "El peso bruto debe ser mayor a cero",
            exception.Message);
    }

    [Fact]
    public void CreateSingle_ShouldCreateTicket()
    {
        // Arrange
        const string ticketNumber = "T-000002";
        const int supplierId = 1;
        const int materialId = 1;
        const string transporter = "Juan Perez";
        const decimal netWeight = 500m;

        // Act
        var ticket = Ticket.CreateSingle(
            ticketNumber,
            supplierId,
            materialId,
            null,
            transporter,
            netWeight);

        // Assert
        Assert.Equal(ticketNumber, ticket.TicketNumber);
        Assert.Equal(supplierId, ticket.SupplierId);
        Assert.Equal(materialId, ticket.MaterialId);
        Assert.Null(ticket.LicensePlate);
        Assert.Equal(transporter, ticket.Transporter);

        Assert.Null(ticket.GrossWeight);
        Assert.Null(ticket.TareWeight);
        Assert.Equal(netWeight, ticket.NetWeight);

        Assert.Equal(
            WeighingType.Single,
            ticket.WeighingType);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);

        Assert.Null(ticket.DepartureAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateSingle_ShouldAllowMissingLicensePlate(string? licensePlate)
    {
        // Act
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            licensePlate,
            "Juan Perez",
            500m);

        // Assert
        Assert.Null(ticket.LicensePlate);
    }

    [Fact]
    public void CreateSingle_ShouldNormalizeLicensePlateWhenProvided()
    {
        // Act
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            "abc1234",
            "Juan Perez",
            500m);

        // Assert
        Assert.Equal(
            "ABC1234",
            ticket.LicensePlate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-500)]
    public void CreateSingle_ShouldThrowWhenNetWeightIsNotPositive(decimal netWeight)
    {
        // Act
        Action act = () => Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            netWeight);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "El peso neto debe ser mayor a cero.",
            exception.Message);
    }

    [Fact]
    public void CreateSingle_ShouldSetCheckInAtAndCreatedAt()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        var after = DateTime.UtcNow;

        // Assert
        Assert.InRange(
            ticket.CheckInAt,
            before,
            after);

        Assert.InRange(
            ticket.CreatedAt,
            before,
            after);

        Assert.Equal(
            ticket.CheckInAt,
            ticket.CreatedAt);

        Assert.Null(ticket.DepartureAt);
    }

    [Fact]
    public void RegisterTare_ShouldRegisterTareAndCalculateNetWeight()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        const decimal tareWeight = 10000m;

        // Act
        ticket.RegisterTare(tareWeight);

        // Assert
        Assert.Equal(tareWeight, ticket.TareWeight);
        Assert.Equal(15000m, ticket.NetWeight);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);
    }

    [Fact]
    public void RegisterTare_ShouldSetDepartureAt()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        var before = DateTime.UtcNow;

        // Act
        ticket.RegisterTare(10000m);

        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(ticket.DepartureAt);

        Assert.InRange(
            ticket.DepartureAt.Value,
            before,
            after);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10000)]
    public void RegisterTare_ShouldThrowWhenTareWeightIsNotPositive(decimal tareWeight)
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        Action act = () =>
            ticket.RegisterTare(tareWeight);

        // Assert
        var exception =
            Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "La tara debe ser mayor a cero.",
            exception.Message);
    }

    [Theory]
    [InlineData(25000)]
    [InlineData(25001)]
    [InlineData(30000)]
    public void RegisterTare_ShouldThrowWhenTareWeightIsGreaterThanOrEqualToGrossWeight(decimal tareWeight)
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        Action act = () =>
            ticket.RegisterTare(tareWeight);

        // Assert
        var exception =
            Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "La tara debe ser menor al peso bruto.",
            exception.Message);
    }

    [Fact]
    public void RegisterTare_ShouldThrowWhenWeighingTypeIsSingle()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000001",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        // Act
        Action act = () =>
            ticket.RegisterTare(100m);

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(
            "La tara solo puede registrarse en un pesaje convencional.",
            exception.Message);
    }

    [Fact]
    public void RegisterTare_ShouldThrowWhenTareIsAlreadyRegistered()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);

        // Act
        Action act = () =>
            ticket.RegisterTare(9000m);

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(
            "El ticket debe estar pendiente de pesaje.",
            exception.Message);
    }

    [Fact]
    public void RegisterAmount_ShouldCalculateAmountAndCompleteConventionalTicket()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);

        const decimal discount = 2m;
        const decimal pricePerKg = 5m;

        // Act
        ticket.RegisterAmount(
            discount,
            pricePerKg);

        // Assert
        Assert.Equal(2m, ticket.Discount);
        Assert.Equal(300m, ticket.DiscountWeight);
        Assert.Equal(14700m, ticket.NetWeightAfterDiscount);
        Assert.Equal(5m, ticket.PricePerKg);
        Assert.Equal(73500m, ticket.Amount);

        Assert.Equal(
            TicketStatus.Completed,
            ticket.Status);
    }

    [Fact]
    public void RegisterAmount_ShouldCalculateAmountAndCompleteSingleTicket()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        // Act
        ticket.RegisterAmount(
            10m,
            5m);

        // Assert
        Assert.Equal(500m, ticket.NetWeight);
        Assert.Equal(10m, ticket.Discount);
        Assert.Equal(50m, ticket.DiscountWeight);
        Assert.Equal(450m, ticket.NetWeightAfterDiscount);
        Assert.Equal(5m, ticket.PricePerKg);
        Assert.Equal(2250m, ticket.Amount);

        Assert.Equal(
            TicketStatus.Completed,
            ticket.Status);
    }

    [Fact]
    public void RegisterAmount_ShouldAllowZeroDiscount()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        // Act
        ticket.RegisterAmount(0m, 5m);

        // Assert
        Assert.Equal(0m, ticket.Discount);
        Assert.Equal(0m, ticket.DiscountWeight);
        Assert.Equal(500m, ticket.NetWeightAfterDiscount);
        Assert.Equal(2500m, ticket.Amount);
        Assert.Equal(TicketStatus.Completed, ticket.Status);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    [InlineData(101)]
    public void RegisterAmount_ShouldThrowWhenDiscountIsOutsideValidRange(
    decimal discount)
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        // Act
        Action act = () =>
            ticket.RegisterAmount(discount, 5m);

        // Assert
        var exception =
            Assert.Throws<ArgumentException>(act);

        Assert.Contains(
            "El descuento debe ser mayor o igual a 0 y menor que 100.",
            exception.Message);

        Assert.Equal(
            "discount",
            exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void RegisterAmount_ShouldThrowWhenPricePerKgIsNotPositive(decimal pricePerKg)
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        // Act
        Action act = () =>
            ticket.RegisterAmount(
                2m,
                pricePerKg);

        // Assert
        var exception =
            Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "El precio por kilogramo debe ser mayor a cero.",
            exception.Message);
    }

    [Fact]
    public void RegisterAmount_ShouldThrowWhenTicketIsWeighingPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        Action act = () =>
            ticket.RegisterAmount(
                2m,
                5m);

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(
            "El ticket debe estar pendiente de importe.",
            exception.Message);
    }

    [Fact]
    public void RegisterAmount_ShouldThrowWhenTicketIsAlreadyCompleted()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        ticket.RegisterAmount(
            2m,
            5m);

        // Act
        Action act = () =>
            ticket.RegisterAmount(
                3m,
                6m);

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(
            "El ticket debe estar pendiente de importe.",
            exception.Message);
    }

    [Fact]
    public void Cancel_ShouldCancelWhenTicketIsWeighingPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        ticket.Cancel();

        // Assert
        Assert.Equal(
            TicketStatus.Cancelled,
            ticket.Status);
    }

    [Fact]
    public void Cancel_ShouldCancelWhenTicketIsAmountPending()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        // Act
        ticket.Cancel();

        // Assert
        Assert.Equal(
            TicketStatus.Cancelled,
            ticket.Status);
    }

    [Fact]
    public void Cancel_ShouldCancelWhenTicketIsCompleted()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        ticket.RegisterAmount(
            2m,
            5m);

        // Act
        ticket.Cancel();

        // Assert
        Assert.Equal(
            TicketStatus.Cancelled,
            ticket.Status);
    }

    [Fact]
    public void Cancel_ShouldThrowWhenTicketIsAlreadyCancelled()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.Cancel();

        // Act
        Action act = () =>
            ticket.Cancel();

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(
            "El ticket ya se encuentra cancelado.",
            exception.Message);
    }

    [Fact]
    public void ChangeSupplier_ShouldUpdateSupplierWhenTicketIsWeighingPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        ticket.ChangeSupplier(2);

        // Assert
        Assert.Equal(2, ticket.SupplierId);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeSupplier_ShouldUpdateSupplierWhenTicketIsCompleted()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        ticket.RegisterAmount(
            0m,
            5m);

        // Act
        ticket.ChangeSupplier(2);

        // Assert
        Assert.Equal(2, ticket.SupplierId);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeSupplier_ShouldThrowWhenTicketIsAmountPending()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        // Act
        Action act = () =>
            ticket.ChangeSupplier(2);

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(
            "El ticket no permite modificar este campo en su estado actual.",
            exception.Message);

        Assert.Equal(1, ticket.SupplierId);
    }

    [Fact]
    public void ChangeSupplier_ShouldThrowWhenTicketIsCancelled()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.Cancel();

        // Act
        Action act = () =>
            ticket.ChangeSupplier(2);

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(
            "El ticket no permite modificar este campo en su estado actual.",
            exception.Message);

        Assert.Equal(1, ticket.SupplierId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeSupplier_ShouldThrowWhenSupplierIdIsInvalid(int supplierId)
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        Action act = () =>
            ticket.ChangeSupplier(supplierId);

        // Assert
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(act);

        Assert.Equal(
            "supplierId",
            exception.ParamName);

        Assert.Contains(
            "El identificador del proveedor debe ser mayor a cero.",
            exception.Message);

        Assert.Equal(1, ticket.SupplierId);
    }

    [Fact]
    public void ChangeMaterial_ShouldUpdateMaterialWhenTicketIsWeighingPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        ticket.ChangeMaterial(2);

        // Assert
        Assert.Equal(2, ticket.MaterialId);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeMaterial_ShouldUpdateMaterialWhenTicketIsCompleted()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        ticket.RegisterAmount(
            0m,
            5m);

        // Act
        ticket.ChangeMaterial(2);

        // Assert
        Assert.Equal(2, ticket.MaterialId);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeMaterial_ShouldThrowWhenTicketIsAmountPending()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        // Act
        Action act = () =>
            ticket.ChangeMaterial(2);

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(
            "El ticket no permite modificar este campo en su estado actual.",
            exception.Message);

        Assert.Equal(1, ticket.MaterialId);
    }

    [Fact]
    public void ChangeMaterial_ShouldThrowWhenTicketIsCancelled()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.Cancel();

        // Act
        Action act = () =>
            ticket.ChangeMaterial(2);

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(
            "El ticket no permite modificar este campo en su estado actual.",
            exception.Message);

        Assert.Equal(1, ticket.MaterialId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeMaterial_ShouldThrowWhenMaterialIdIsInvalid(int materialId)
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        Action act = () =>
            ticket.ChangeMaterial(materialId);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);

        Assert.Equal(1, ticket.MaterialId);
    }

    [Fact]
    public void ChangeLicensePlate_ShouldUpdateLicensePlateWhenConventionalTicketIsWeighingPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        ticket.ChangeLicensePlate("xyz5678");

        // Assert
        Assert.Equal("XYZ5678", ticket.LicensePlate);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeLicensePlate_ShouldThrowWhenConventionalLicensePlateIsMissing()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        Action act = () =>
            ticket.ChangeLicensePlate(null);

        // Assert
        var exception =
            Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "Las placas son obligatorias.",
            exception.Message);

        Assert.Equal("ABC1234", ticket.LicensePlate);
    }

    [Fact]
    public void ChangeLicensePlate_ShouldUpdateLicensePlateWhenSingleTicketIsCompleted()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        ticket.RegisterAmount(0m, 5m);

        // Act
        ticket.ChangeLicensePlate("abc1234");

        // Assert
        Assert.Equal("ABC1234", ticket.LicensePlate);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeLicensePlate_ShouldAllowRemovingLicensePlateFromSingleTicket()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            500m);

        ticket.RegisterAmount(0m, 5m);

        // Act
        ticket.ChangeLicensePlate(null);

        // Assert
        Assert.Null(ticket.LicensePlate);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeLicensePlate_ShouldNormalizeMissingLicensePlateToNullForSingleTicket(string licensePlate)
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            500m);

        ticket.RegisterAmount(0m, 5m);

        // Act
        ticket.ChangeLicensePlate(licensePlate);

        // Assert
        Assert.Null(ticket.LicensePlate);
    }

    [Fact]
    public void ChangeLicensePlate_ShouldThrowWhenTicketIsAmountPending()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        // Act
        Action act = () =>
            ticket.ChangeLicensePlate("ABC1234");

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Null(ticket.LicensePlate);
    }

    [Fact]
    public void ChangeLicensePlate_ShouldThrowWhenTicketIsCancelled()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.Cancel();

        // Act
        Action act = () =>
            ticket.ChangeLicensePlate("XYZ5678");

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Equal("ABC1234", ticket.LicensePlate);
    }

    [Fact]
    public void ChangeTransporter_ShouldUpdateTransporterWhenTicketIsWeighingPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        ticket.ChangeTransporter("maria lopez");

        // Assert
        Assert.Equal("Maria Lopez", ticket.Transporter);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeTransporter_ShouldUpdateTransporterWhenTicketIsCompleted()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        ticket.RegisterAmount(0m, 5m);

        // Act
        ticket.ChangeTransporter("maria lopez");

        // Assert
        Assert.Equal("Maria Lopez", ticket.Transporter);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeTransporter_ShouldThrowWhenTicketIsAmountPending()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        // Act
        Action act = () =>
            ticket.ChangeTransporter("Maria Lopez");

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Equal("Juan Perez", ticket.Transporter);
    }

    [Fact]
    public void ChangeTransporter_ShouldThrowWhenTicketIsCancelled()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.Cancel();

        // Act
        Action act = () =>
            ticket.ChangeTransporter("Maria Lopez");

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Equal("Juan Perez", ticket.Transporter);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeTransporter_ShouldThrowWhenTransporterIsEmpty(string? transporter)
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        Action act = () =>
            ticket.ChangeTransporter(transporter!);

        // Assert
        Assert.Throws<ArgumentException>(act);

        Assert.Equal("Juan Perez", ticket.Transporter);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeGrossWeight_ShouldUpdateGrossWeightWhenTicketIsWeighingPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        ticket.ChangeGrossWeight(26000m);

        // Assert
        Assert.Equal(26000m, ticket.GrossWeight);
        Assert.Null(ticket.TareWeight);
        Assert.Null(ticket.NetWeight);
        Assert.Equal(TicketStatus.WeighingPending, ticket.Status);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeGrossWeight_ShouldThrowWhenGrossWeightIsNotPositive(
    decimal grossWeight)
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        Action act = () =>
            ticket.ChangeGrossWeight(grossWeight);

        // Assert
        Assert.Throws<ArgumentException>(act);

        Assert.Equal(25000m, ticket.GrossWeight);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeGrossWeight_ShouldRecalculateValuesWhenTicketIsCompleted()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);
        ticket.RegisterAmount(2m, 5m);

        // Act
        ticket.ChangeGrossWeight(26000m);

        // Assert
        Assert.Equal(26000m, ticket.GrossWeight);
        Assert.Equal(10000m, ticket.TareWeight);

        Assert.Equal(16000m, ticket.NetWeight);

        Assert.Equal(2m, ticket.Discount);
        Assert.Equal(320m, ticket.DiscountWeight);
        Assert.Equal(15680m, ticket.NetWeightAfterDiscount);

        Assert.Equal(5m, ticket.PricePerKg);
        Assert.Equal(78400m, ticket.Amount);

        Assert.Equal(TicketStatus.Completed, ticket.Status);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeGrossWeight_ShouldThrowWhenNewGrossWeightIsLessThanOrEqualToTareWeight()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);
        ticket.RegisterAmount(2m, 5m);

        // Act
        Action act = () =>
            ticket.ChangeGrossWeight(9000m);

        // Assert
        Assert.Throws<ArgumentException>(act);

        Assert.Equal(25000m, ticket.GrossWeight);
        Assert.Equal(10000m, ticket.TareWeight);
        Assert.Equal(15000m, ticket.NetWeight);
        Assert.Equal(300m, ticket.DiscountWeight);
        Assert.Equal(14700m, ticket.NetWeightAfterDiscount);
        Assert.Equal(73500m, ticket.Amount);

        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeGrossWeight_ShouldThrowWhenTicketIsAmountPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);

        // Act
        Action act = () =>
            ticket.ChangeGrossWeight(26000m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(25000m, ticket.GrossWeight);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeGrossWeight_ShouldThrowWhenTicketIsCancelled()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.Cancel();

        // Act
        Action act = () =>
            ticket.ChangeGrossWeight(26000m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(25000m, ticket.GrossWeight);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeGrossWeight_ShouldThrowWhenTicketIsSingleWeighing()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000002",
            1,
            1,
            null,
            "Juan Perez",
            500m);

        // Act
        Action act = () =>
            ticket.ChangeGrossWeight(600m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Null(ticket.GrossWeight);
        Assert.Equal(500m, ticket.NetWeight);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeTareWeight_ShouldUpdateTareAndNetWeightWhenTicketIsAmountPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);

        // Act
        ticket.ChangeTareWeight(11000m);

        // Assert
        Assert.Equal(11000m, ticket.TareWeight);
        Assert.Equal(14000m, ticket.NetWeight);

        Assert.Equal(
            TicketStatus.AmountPending,
            ticket.Status);

        Assert.NotNull(ticket.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(25000)]
    [InlineData(26000)]
    public void ChangeTareWeight_ShouldThrowWhenTareWeightIsInvalid(decimal tareWeight)
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);

        // Act
        Action act = () =>
            ticket.ChangeTareWeight(tareWeight);

        // Assert
        Assert.Throws<ArgumentException>(act);

        Assert.Equal(10000m, ticket.TareWeight);
        Assert.Equal(15000m, ticket.NetWeight);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeDiscount_ShouldUpdateDiscountAndRecalculateValuesWhenTicketIsCompleted()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);
        ticket.RegisterAmount(2m, 5m);

        // Act
        ticket.ChangeDiscount(5m);

        // Assert
        Assert.Equal(5m, ticket.Discount);
        Assert.Equal(750m, ticket.DiscountWeight);
        Assert.Equal(14250m, ticket.NetWeightAfterDiscount);

        Assert.Equal(5m, ticket.PricePerKg);
        Assert.Equal(71250m, ticket.Amount);

        Assert.Equal(TicketStatus.Completed, ticket.Status);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    [InlineData(101)]
    public void ChangeDiscount_ShouldThrowWhenDiscountIsOutsideValidRange(decimal discount)
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);
        ticket.RegisterAmount(2m, 5m);

        // Act
        Action act = () =>
            ticket.ChangeDiscount(discount);

        // Assert
        Assert.Throws<ArgumentException>(act);

        Assert.Equal(2m, ticket.Discount);
        Assert.Equal(300m, ticket.DiscountWeight);
        Assert.Equal(14700m, ticket.NetWeightAfterDiscount);
        Assert.Equal(73500m, ticket.Amount);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeDiscount_ShouldAllowZeroDiscount()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);
        ticket.RegisterAmount(2m, 5m);

        // Act
        ticket.ChangeDiscount(0m);

        // Assert
        Assert.Equal(0m, ticket.Discount);
        Assert.Equal(0m, ticket.DiscountWeight);
        Assert.Equal(15000m, ticket.NetWeightAfterDiscount);
        Assert.Equal(75000m, ticket.Amount);

        Assert.Equal(TicketStatus.Completed, ticket.Status);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeDiscount_ShouldThrowWhenTicketIsWeighingPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        Action act = () =>
            ticket.ChangeDiscount(5m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Null(ticket.Discount);
        Assert.Null(ticket.DiscountWeight);
        Assert.Null(ticket.NetWeightAfterDiscount);
        Assert.Null(ticket.Amount);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeDiscount_ShouldThrowWhenTicketIsAmountPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);

        // Act
        Action act = () =>
            ticket.ChangeDiscount(5m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Null(ticket.Discount);
        Assert.Null(ticket.DiscountWeight);
        Assert.Null(ticket.NetWeightAfterDiscount);
        Assert.Null(ticket.Amount);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeDiscount_ShouldThrowWhenTicketIsCancelled()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);
        ticket.RegisterAmount(2m, 5m);
        ticket.Cancel();

        // Act
        Action act = () =>
            ticket.ChangeDiscount(5m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(2m, ticket.Discount);
        Assert.Equal(300m, ticket.DiscountWeight);
        Assert.Equal(14700m, ticket.NetWeightAfterDiscount);
        Assert.Equal(73500m, ticket.Amount);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangePricePerKg_ShouldUpdatePriceAndRecalculateAmountWhenTicketIsCompleted()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);
        ticket.RegisterAmount(2m, 5m);

        // Act
        ticket.ChangePricePerKg(6m);

        // Assert
        Assert.Equal(6m, ticket.PricePerKg);

        Assert.Equal(300m, ticket.DiscountWeight);
        Assert.Equal(14700m, ticket.NetWeightAfterDiscount);
        Assert.Equal(88200m, ticket.Amount);

        Assert.Equal(TicketStatus.Completed, ticket.Status);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangePricePerKg_ShouldThrowWhenPricePerKgIsNotPositive(
    decimal pricePerKg)
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);
        ticket.RegisterAmount(2m, 5m);

        // Act
        Action act = () =>
            ticket.ChangePricePerKg(pricePerKg);

        // Assert
        Assert.Throws<ArgumentException>(act);

        Assert.Equal(5m, ticket.PricePerKg);
        Assert.Equal(300m, ticket.DiscountWeight);
        Assert.Equal(14700m, ticket.NetWeightAfterDiscount);
        Assert.Equal(73500m, ticket.Amount);

        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangePricePerKg_ShouldThrowWhenTicketIsWeighingPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        // Act
        Action act = () =>
            ticket.ChangePricePerKg(6m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Null(ticket.PricePerKg);
        Assert.Null(ticket.Amount);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangePricePerKg_ShouldThrowWhenTicketIsAmountPending()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);

        // Act
        Action act = () =>
            ticket.ChangePricePerKg(6m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Null(ticket.PricePerKg);
        Assert.Null(ticket.Amount);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangePricePerKg_ShouldThrowWhenTicketIsCancelled()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);
        ticket.RegisterAmount(2m, 5m);
        ticket.Cancel();

        // Act
        Action act = () =>
            ticket.ChangePricePerKg(6m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(5m, ticket.PricePerKg);
        Assert.Equal(300m, ticket.DiscountWeight);
        Assert.Equal(14700m, ticket.NetWeightAfterDiscount);
        Assert.Equal(73500m, ticket.Amount);

        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeNetWeight_ShouldUpdateNetWeightWhenSingleTicketIsAmountPending()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000001",
            1,
            1,
            null,
            "Juan Perez",
            15000m);

        // Act
        ticket.ChangeNetWeight(14000m);

        // Assert
        Assert.Equal(14000m, ticket.NetWeight);
        Assert.Equal(TicketStatus.AmountPending, ticket.Status);

        Assert.Null(ticket.Discount);
        Assert.Null(ticket.DiscountWeight);
        Assert.Null(ticket.NetWeightAfterDiscount);
        Assert.Null(ticket.PricePerKg);
        Assert.Null(ticket.Amount);

        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeNetWeight_ShouldUpdateNetWeightAndRecalculateAmountWhenSingleTicketIsCompleted()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000001",
            1,
            1,
            null,
            "Juan Perez",
            15000m);

        ticket.RegisterAmount(
            2m,
            5m);

        // Act
        ticket.ChangeNetWeight(14000m);

        // Assert
        Assert.Equal(14000m, ticket.NetWeight);

        Assert.Equal(2m, ticket.Discount);
        Assert.Equal(280m, ticket.DiscountWeight);
        Assert.Equal(13720m, ticket.NetWeightAfterDiscount);

        Assert.Equal(5m, ticket.PricePerKg);
        Assert.Equal(68600m, ticket.Amount);

        Assert.Equal(TicketStatus.Completed, ticket.Status);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeNetWeight_ShouldThrowWhenNetWeightIsLessThanOrEqualToZero(
    decimal netWeight)
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000001",
            1,
            1,
            null,
            "Juan Perez",
            15000m);

        // Act
        Action act = () =>
            ticket.ChangeNetWeight(netWeight);

        // Assert
        var exception =
            Assert.Throws<ArgumentException>(act);

        Assert.Equal(
            "El peso neto debe ser mayor a cero.",
            exception.Message);

        Assert.Equal(15000m, ticket.NetWeight);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeNetWeight_ShouldThrowWhenSingleTicketIsCancelled()
    {
        // Arrange
        var ticket = Ticket.CreateSingle(
            "T-000001",
            1,
            1,
            null,
            "Juan Perez",
            15000m);

        ticket.Cancel();

        // Act
        Action act = () =>
            ticket.ChangeNetWeight(14000m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(15000m, ticket.NetWeight);
        Assert.Equal(TicketStatus.Cancelled, ticket.Status);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeNetWeight_ShouldThrowWhenTicketIsConventional()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);

        // Act
        Action act = () =>
            ticket.ChangeNetWeight(14000m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(15000m, ticket.NetWeight);
        Assert.Null(ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeNetWeight_ShouldThrowWhenCompletedTicketIsConventional()
    {
        // Arrange
        var ticket = Ticket.CreateConventional(
            "T-000001",
            1,
            1,
            "ABC1234",
            "Juan Perez",
            25000m);

        ticket.RegisterTare(10000m);
        ticket.RegisterAmount(2m, 5m);

        // Act
        Action act = () =>
            ticket.ChangeNetWeight(14000m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);

        Assert.Equal(15000m, ticket.NetWeight);
        Assert.Equal(73500m, ticket.Amount);
        Assert.Null(ticket.UpdatedAt);
    }
}