using PurchaseTicket.Application.Abstractions.Devices;
using PurchaseTicket.Infrastructure.Weighing;
using PurchaseTicket.Infrastructure.Weighing.Serial;

namespace PurchaseTicket.Infrastructure.Tests.Weighing;

public class WeightIndicatorSerialPortFactoryTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowDeviceNotConfigured_WhenPortIsMissing(string portName)
    {
        // Arrange
        var factory = new WeightIndicatorSerialPortFactory(
            new WeightIndicatorOptions { PortName = portName });

        // Act y Assert
        Assert.Throws<DeviceNotConfiguredException>(() => factory.Create());
    }
}