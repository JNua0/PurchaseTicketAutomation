using PurchaseTicket.Application.Abstractions.Devices;
using PurchaseTicket.Infrastructure.Printing;
using PurchaseTicket.Infrastructure.Printing.Serial;

namespace PurchaseTicket.Infrastructure.Tests.Printing;

public class SerialPortFactoryTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowDeviceNotConfigured_WhenPortIsMissing(string portName)
    {
        // Arrange
        var factory = new SerialPortFactory(
            new SerialPrinterOptions { PortName = portName });

        // Act y Assert
        Assert.Throws<DeviceNotConfiguredException>(() => factory.Create());
    }
}