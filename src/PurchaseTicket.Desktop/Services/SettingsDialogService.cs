using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PurchaseTicket.Desktop.Configuration;
using PurchaseTicket.Desktop.ViewModels.Settings;
using PurchaseTicket.Desktop.Views.Settings;

namespace PurchaseTicket.Desktop.Services;

public class SettingsDialogService : ISettingsDialogService
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;

    // Constructor

    public SettingsDialogService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Metodos publicos

    // Cada apertura crea una ventana nueva con el encabezado guardado
    public TicketHeaderSettings? ShowTicketHeaderDialog(TicketHeaderSettings current)
    {
        using var scope = _scopeFactory.CreateScope();

        var window = scope.ServiceProvider
            .GetRequiredService<TicketHeaderWindow>();

        var viewModel = (TicketHeaderFormViewModel)window.DataContext;

        viewModel.Initialize(current);

        window.Owner = System.Windows.Application.Current.MainWindow;

        return window.ShowDialog() == true
            ? viewModel.SavedHeader
            : null;
    }

    public string? ShowBackupFolderDialog(string currentFolder)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Carpeta de respaldos",
            Multiselect = false
        };

        if (Directory.Exists(currentFolder))
            dialog.InitialDirectory = currentFolder;

        return dialog.ShowDialog(System.Windows.Application.Current.MainWindow) == true
            ? dialog.FolderName
            : null;
    }

    public void OpenFolder(string folder)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = folder,
            UseShellExecute = true
        });
    }
}