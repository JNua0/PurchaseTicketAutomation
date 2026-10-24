using PurchaseTicket.Infrastructure.Backups;

namespace PurchaseTicket.Infrastructure.Tests.Backups;

// Solo los mensajes: crear un respaldo de verdad necesita Docker
public class DockerPgDumpBackupServiceTests
{
    private const string Container = "purchase-ticket-postgres";

    [Fact]
    public void DescribeError_ShouldExplainMissingContainer()
    {
        string message = DockerPgDumpBackupService.DescribeError(
            "Error response from daemon: No such container: purchase-ticket-postgres",
            Container);

        Assert.Equal(
            "No se encontró el contenedor purchase-ticket-postgres. Revisa en Docker Desktop que la base de datos exista.",
            message);
    }

    [Fact]
    public void DescribeError_ShouldExplainStoppedContainer()
    {
        string message = DockerPgDumpBackupService.DescribeError(
            "Error response from daemon: container 3f2a is not running",
            Container);

        Assert.Equal(
            "El contenedor de la base de datos está detenido. Inícialo en Docker Desktop y vuelve a intentar.",
            message);
    }

    [Fact]
    public void DescribeError_ShouldExplainClosedDockerDesktop()
    {
        string message = DockerPgDumpBackupService.DescribeError(
            "error during connect: Get \"http://%2F%2F.%2Fpipe%2FdockerDesktopLinuxEngine/v1.47/containers\": open //./pipe/docker_engine: The system cannot find the file specified.",
            Container);

        Assert.Equal("Docker Desktop no está abierto. Ábrelo y vuelve a intentar.", message);
    }

    [Fact]
    public void DescribeError_ShouldShowFirstLineOfOtherErrors()
    {
        string message = DockerPgDumpBackupService.DescribeError(
            "\npg_dump: error: connection to server failed: FATAL:  database \"x\" does not exist\nmas detalle",
            Container);

        Assert.Equal(
            "El respaldo terminó con un error: pg_dump: error: connection to server failed: FATAL:  database \"x\" does not exist",
            message);
    }

    [Fact]
    public void DescribeError_ShouldHandleEmptyOutput()
    {
        Assert.Equal(
            "El respaldo terminó con un error desconocido.",
            DockerPgDumpBackupService.DescribeError(string.Empty, Container));
    }
}
