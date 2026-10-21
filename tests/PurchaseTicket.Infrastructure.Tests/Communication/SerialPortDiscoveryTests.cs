using Microsoft.Extensions.Logging.Abstractions;
using PurchaseTicket.Infrastructure.Communication.Serial;

namespace PurchaseTicket.Infrastructure.Tests.Communication;

public class SerialPortDiscoveryTests
{
    [Fact]
    public void GetPorts_ShouldAddDescriptionsAndSortByNumber()
    {
        // Arrange
        var discovery = new SerialPortDiscovery(
            () => ["COM10", "COM3", "com5"],
            () =>
            [
                "USB-SERIAL CH340 (COM5)",
                "Puerto de comunicaciones (COM3)",

                // Un dispositivo que ya no esta en la lista de puertos se ignora
                "Bluetooth (COM20)"
            ],
            NullLogger<SerialPortDiscovery>.Instance);

        // Act
        var ports = discovery.GetPorts();

        // Assert
        Assert.Equal(["COM3", "COM5", "COM10"], ports.Select(port => port.Name));

        Assert.Equal("Puerto de comunicaciones", ports[0].Description);
        Assert.Equal("USB-SERIAL CH340", ports[1].Description);
        Assert.Null(ports[2].Description);
    }

    [Fact]
    public void GetPorts_ShouldListPortsWithoutDescription_WhenDeviceQueryFails()
    {
        // Arrange
        var discovery = new SerialPortDiscovery(
            () => ["COM4"],
            () => throw new InvalidOperationException("WMI no disponible."),
            NullLogger<SerialPortDiscovery>.Instance);

        // Act
        var ports = discovery.GetPorts();

        // Assert
        var port = Assert.Single(ports);

        Assert.Equal("COM4", port.Name);
        Assert.Null(port.Description);
    }

    [Fact]
    public void GetPorts_ShouldReturnEmptyList_WhenNoPortIsConnected()
    {
        // Arrange
        var discovery = new SerialPortDiscovery(
            () => [],
            () => [],
            NullLogger<SerialPortDiscovery>.Instance);

        // Act
        var ports = discovery.GetPorts();

        // Assert
        Assert.Empty(ports);
    }
}