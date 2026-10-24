using System.Diagnostics;
using Microsoft.Win32;

namespace PurchaseTicket.Desktop.Services;

public sealed class ReportsDialogService : IReportsDialogService
{
    // Metodos publicos

    public string? ShowSaveDialog(string suggestedName, string extension)
    {
        string filter = extension == "pdf"
            ? "Documento PDF (*.pdf)|*.pdf"
            : "Libro de Excel (*.xlsx)|*.xlsx";

        var dialog = new SaveFileDialog
        {
            Title = "Guardar reporte",
            FileName = suggestedName,
            DefaultExt = extension,
            AddExtension = true,
            Filter = filter,
            OverwritePrompt = true
        };

        return dialog.ShowDialog(System.Windows.Application.Current.MainWindow) == true
            ? dialog.FileName
            : null;
    }

    public void OpenFile(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }
}
