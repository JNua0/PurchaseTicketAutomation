
using System.Windows;
using PurchaseTicket.Desktop.ViewModels;

namespace PurchaseTicket.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();

        DataContext = mainViewModel;
    }
}