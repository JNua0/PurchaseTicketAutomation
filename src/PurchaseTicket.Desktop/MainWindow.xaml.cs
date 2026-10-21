using System.Windows;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels;

namespace PurchaseTicket.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(
        MainViewModel mainViewModel,
        NotificationService notifications)
    {
        InitializeComponent();

        DataContext = mainViewModel;

        // El aviso se enlaza al servicio, no a MainViewModel
        NotificationToast.DataContext = notifications;
    }
}