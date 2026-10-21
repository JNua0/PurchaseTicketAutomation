using System.Windows;
using System.Windows.Input;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.Settings;

namespace PurchaseTicket.Desktop.Views.Settings;

public partial class TicketHeaderWindow : Window
{
    // Constructor

    public TicketHeaderWindow(TicketHeaderFormViewModel viewModel)
    {
        CancelCommand = new RelayCommand(Cancel);

        InitializeComponent();

        DataContext = viewModel;

        viewModel.Saved += OnSaved;
    }

    // Propiedades

    public ICommand CancelCommand { get; }

    // Metodos privados

    // Cerrar la ventana cuando el encabezado se guarda
    private void OnSaved(object? sender, EventArgs e)
    {
        DialogResult = true;
    }

    // Guardar es instantaneo, asi que no hay que esperar para cancelar
    private void Cancel()
    {
        DialogResult = false;
    }
}