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
        Assert.Equal(name, supplier.Name);
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
            "ACEROS ABC S.A. DE C.V.",
            supplier.Name
        );
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
    [InlineData("5512345678")]
    [InlineData("+525512345678")]
    public void Constructor_ShouldAcceptValidPhoneNumber(string phoneNumber)
    {
        // Act
        var supplier = new Supplier("Proveedor de prueba", phoneNumber);

        // Assert
        Assert.Equal(phoneNumber, supplier.PhoneNumber);
    }

    [Theory]
    [InlineData("551234567")]
    [InlineData("55123456789")]
    [InlineData("525512345678")]
    [InlineData("+15512345678")]
    [InlineData("55 1234 5678")]
    [InlineData("55-1234-5678")]
    [InlineData("abcdefghij")]
    public void Constructor_ShouldThrow_WhenPhoneNumberIsInvalid(string phoneNumber)
    {
        // Act
        Action act = () => new Supplier("Proveedor de prueba", phoneNumber);

        // Assert
        Assert.Throws<ArgumentException>(act);
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
        Assert.Equal("Proveedor actualizado", supplier.Name);
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
        Assert.Equal("Proveedor original", supplier.Name);
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
        Assert.Equal("Proveedor original", supplier.Name);
    }

    [Theory]
    [InlineData("551234567")]
    [InlineData("55123456789")]
    [InlineData("525512345678")]
    [InlineData("+15512345678")]
    [InlineData("55 1234 5678")]
    [InlineData("55-1234-5678")]
    [InlineData("abcdefghij")]
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
