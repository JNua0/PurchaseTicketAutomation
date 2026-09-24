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

    [Fact]
    public void Constructor_ShouldCreateActiveMaterial()
    {
        // Arrange & Act
        var material = new Material("Material de prueba");

        // Assert
        Assert.True(material.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldSetMaterialAsInactive()
    {
        // Arrange
        var material = new Material("Material de prueba");

        // Act
        material.Deactivate();

        // Assert
        Assert.False(material.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetMaterialAsActive()
    {
        // Arrange
        var material = new Material("Material de prueba");
        material.Deactivate();

        // Act
        material.Activate();

        // Assert
        Assert.True(material.IsActive);
    }

    [Theory]
    [InlineData("aluminio", "Aluminio")]
    [InlineData("ALUMINIO", "Aluminio")]
    [InlineData("ALUMINIO RECICLADO", "Aluminio reciclado")]
    public void UpdateName_ShouldNormalizeMaterialName(
    string name,
    string expectedName)
    {
        // Arrange
        var material = new Material("Acero");

        // Act
        material.UpdateName(name);

        // Assert
        Assert.Equal(expectedName, material.Name);
    }

    [Fact]
    public void UpdateName_ShouldThrow_WhenNameIsInvalid()
    {
        // Arrange
        var material = new Material("Material original");

        // Act
        Action act = () => material.UpdateName("");

        // Assert
        Assert.Throws<ArgumentException>(act);
        Assert.Equal("Material original", material.Name);
    }

    [Theory]
    [InlineData("acero", "Acero")]
    [InlineData("ACERO", "Acero")]
    [InlineData("aCERO", "Acero")]
    [InlineData("ACERO INOXIDABLE", "Acero inoxidable")]
    [InlineData("acERO InOxIdAbLe", "Acero inoxidable")]
    public void Constructor_ShouldNormalizeMaterialName(
    string name,
    string expectedName)
    {
        // Act
        var material = new Material(name);

        // Assert
        Assert.Equal(expectedName, material.Name);
    }
}