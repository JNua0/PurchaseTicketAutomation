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

        // Los avisos se enlazan al servicio, no a MainViewModel
        NotificationHost.DataContext = notifications;
    }
}