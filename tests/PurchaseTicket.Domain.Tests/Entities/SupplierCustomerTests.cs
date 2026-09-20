using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Domain.Tests.Entities;

public class SupplierCustomerTests
{
    [Fact]
    public void Constructor_ShouldCreateSupplierCustomerWithProvidedName()
    {
        // Arrange
        const string name = "ACEROS ABC S.A. DE C.V.";

        // Act
        var supplierCustomer = new SupplierCustomer(name);

        // Assert
        Assert.Equal(name, supplierCustomer.Name);
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        // Arrange
        const string name = "  ACEROS ABC S.A. DE C.V.  ";

        // Act
        var supplierCustomer = new SupplierCustomer(name);

        // Assert
        Assert.Equal(
            "ACEROS ABC S.A. DE C.V.",
            supplierCustomer.Name
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
        Action act = () => new SupplierCustomer(name);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenNameExceedsMaximumLength()
    {
        // Arrange
        string name = new('A', 51);

        // Act
        Action act = () => new SupplierCustomer(name);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
}
