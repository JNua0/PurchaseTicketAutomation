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
}
