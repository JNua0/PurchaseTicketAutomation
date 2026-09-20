using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Domain.Tests.Entities;

public class MaterialTests
{
    [Fact]
    public void Constructor_ShouldCreateMaterialWithProvidedName()
    {
        // Arrange
        const string name = "Steel";

        // Act
        var material = new Material(name);

        // Assert
        Assert.Equal(name, material.Name);
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        // Arrange
        const string name = "  Steel  ";

        // Act
        var material = new Material(name);

        // Assert
        Assert.Equal("Steel", material.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Constructor_ShouldThrowWhenNameIsEmptyOrWhitespace(
        string name)
    {
        // Act
        Action act = () => new Material(name);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenNameExceedsMaximumLength()
    {
        // Arrange
        string name = new('A', 51);

        // Act
        Action act = () => new Material(name);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
}