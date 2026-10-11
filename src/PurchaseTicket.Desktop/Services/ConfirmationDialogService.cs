using System.Windows;

namespace PurchaseTicket.Desktop.Services;

public class ConfirmationDialogService : IConfirmationDialogService
{
    public bool Confirm(string title, string message)
    {
        var result = MessageBox.Show(
            System.Windows.Application.Current.MainWindow,
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);

        return result == MessageBoxResult.Yes;
    }
}
