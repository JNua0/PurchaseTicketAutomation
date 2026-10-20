using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Domain.Tests.Entities;

public class SupplierTests
{
    [Fact]
    public void Constructor_ShouldCreateSupplierWithProvidedName()
    {
        // Arrange
        const string name = "ACEROS ABC S.A. DE C.V.";

        // Act
        var supplier = new Supplier(name);

        // Assert
        Assert.Equal("Aceros Abc S.A. De C.V.", supplier.Name);
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        // Arrange
        const string name = "  ACEROS ABC S.A. DE C.V.  ";

        // Act
        var supplier = new Supplier(name);

        // Assert
        Assert.Equal(
            "Aceros Abc S.A. De C.V.",
            supplier.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Constructor_ShouldThrowWhenNameIsEmptyOrWhitespace(
        string name)
    {
        // Act
        Action act = () => new Supplier(name);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenNameExceedsMaximumLength()
    {
        // Arrange
        string name = new('A', 51);

        // Act
        Action act = () => new Supplier(name);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_ShouldCreateActiveSupplier()
    {
        // Arrange & Act
        var supplier = new Supplier("Proveedor de prueba");

        // Assert
        Assert.True(supplier.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldSetSupplierAsInactive()
    {
        // Arrange
        var supplier = new Supplier("Proveedor de prueba");

        // Act
        supplier.Deactivate();

        // Assert
        Assert.False(supplier.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetSupplierAsActive()
    {
        // Arrange
        var supplier = new Supplier("Proveedor de prueba");
        supplier.Deactivate();

        // Act
        supplier.Activate();

        // Assert
        Assert.True(supplier.IsActive);
    }

    [Fact]
    public void Constructor_ShouldCreateSupplierWithPhoneNumber()
    {
        // Arrange
        const string phoneNumber = "5512345678";

        // Act
        var supplier = new Supplier("Proveedor de prueba", phoneNumber);

        // Assert
        Assert.Equal(phoneNumber, supplier.PhoneNumber);
    }

    [Theory]
    [InlineData("5512345678", "5512345678")]
    [InlineData("+525512345678", "+525512345678")]
    [InlineData("55 1234 5678", "5512345678")]
    [InlineData("55-1234-5678", "5512345678")]
    [InlineData("+52 55 1234 5678", "+525512345678")]
    [InlineData("+1 555 123 4567", "+15551234567")]
    [InlineData("525512345678", "525512345678")]
    [InlineData("+123456789012345", "+123456789012345")]
    public void Constructor_ShouldAcceptValidPhoneNumber(
        string phoneNumber,
        string expected)
    {
        // Act
        var supplier = new Supplier("Proveedor de prueba", phoneNumber);

        // Assert
        Assert.Equal(expected, supplier.PhoneNumber);
    }

    [Theory]
    [InlineData("551234567")]
    [InlineData("1234567890123456")]
    [InlineData("abcdefghij")]
    [InlineData("55.1234.5678")]
    [InlineData("(55) 1234 5678")]
    [InlineData("55+12345678")]
    [InlineData("++525512345678")]
    [InlineData("+")]
    public void Constructor_ShouldThrow_WhenPhoneNumberIsInvalid(string phoneNumber)
    {
        // Act
        Action act = () => new Supplier("Proveedor de prueba", phoneNumber);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);

        Assert.Contains(
            "El teléfono debe tener entre 10 y 15 dígitos y solo puede iniciar con +.",
            exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldSetPhoneNumberToNull_WhenPhoneNumberIsEmpty(
    string? phoneNumber)
    {
        // Act
        var supplier = new Supplier("Proveedor de prueba", phoneNumber);

        // Assert
        Assert.Null(supplier.PhoneNumber);
    }

    [Fact]
    public void Constructor_ShouldTrimPhoneNumber()
    {
        // Arrange
        const string phoneNumber = "  5512345678  ";

        // Act
        var supplier = new Supplier("Proveedor de prueba", phoneNumber);

        // Assert
        Assert.Equal("5512345678", supplier.PhoneNumber);
    }

    [Fact]
    public void UpdateName_ShouldUpdateSupplierName()
    {
        // Arrange
        var supplier = new Supplier(
            "Proveedor original",
            "5512345678");

        // Act
        supplier.UpdateName("Proveedor actualizado");

        // Assert
        Assert.Equal("Proveedor Actualizado", supplier.Name);
        Assert.Equal("5512345678", supplier.PhoneNumber);
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldUpdateSupplierPhoneNumber()
    {
        // Arrange
        var supplier = new Supplier(
            "Proveedor original",
            "5512345678");

        // Act
        supplier.UpdatePhoneNumber("+525512345678");

        // Assert
        Assert.Equal("Proveedor Original", supplier.Name);
        Assert.Equal("+525512345678", supplier.PhoneNumber);
    }

    [Fact]
    public void UpdateName_ShouldThrow_WhenNameIsInvalid()
    {
        // Arrange
        var supplier = new Supplier("Proveedor original");

        // Act
        Action act = () => supplier.UpdateName("");

        // Assert
        Assert.Throws<ArgumentException>(act);
        Assert.Equal("Proveedor Original", supplier.Name);
    }

    [Theory]
    [InlineData("551234567")]
    [InlineData("1234567890123456")]
    [InlineData("abcdefghij")]
    [InlineData("55.1234.5678")]
    [InlineData("(55) 1234 5678")]
    [InlineData("55+12345678")]
    [InlineData("++525512345678")]
    [InlineData("+")]
    public void UpdatePhoneNumber_ShouldThrow_WhenPhoneNumberIsInvalid(
    string phoneNumber)
    {
        // Arrange
        var supplier = new Supplier(
            "Proveedor de prueba",
            "5512345678");

        // Act
        Action act = () => supplier.UpdatePhoneNumber(phoneNumber);

        // Assert
        Assert.Throws<ArgumentException>(act);
        Assert.Equal("5512345678", supplier.PhoneNumber);
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldRemovePhoneNumber_WhenNull()
    {
        // Arrange
        var supplier = new Supplier(
            "Proveedor de prueba",
            "5512345678");

        // Act
        supplier.UpdatePhoneNumber(null);

        // Assert
        Assert.Null(supplier.PhoneNumber);
    }
}
