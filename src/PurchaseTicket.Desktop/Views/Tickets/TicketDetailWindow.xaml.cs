using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.Tickets;

namespace PurchaseTicket.Desktop.Views.Tickets;

public partial class TicketDetailWindow : Window
{
    // Campos

    private readonly TicketDetailViewModel _viewModel;

    // Constructor

    public TicketDetailWindow(TicketDetailViewModel viewModel)
    {
        CloseCommand = new RelayCommand(Close);

        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    // Propiedades

    public ICommand CloseCommand { get; }

    // Metodos privados

    // Se carga ya abierta: la ventana aparece de inmediato con "Cargando ticket..."
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }

    // CenterOwner centra la ventana al abrirse, cuando aun no carga el ticket.
    // Al llegar los datos crece hacia abajo y se saldria de la pantalla: se
    // vuelve a centrar cada vez que cambia de tamano
    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!e.HeightChanged)
            return;

        Rect workArea = SystemParameters.WorkArea;

        // Una ventana principal maximizada reporta su posicion fuera del
        // area visible; en ese caso se centra en el area de trabajo
        Rect ownerBounds = Owner is null || Owner.WindowState == WindowState.Maximized
            ? workArea
            : new Rect(Owner.Left, Owner.Top, Owner.ActualWidth, Owner.ActualHeight);

        double top = ownerBounds.Top + ((ownerBounds.Height - ActualHeight) / 2);

        // Nunca por encima ni por debajo del area de trabajo
        Top = Math.Max(workArea.Top, Math.Min(top, workArea.Bottom - ActualHeight));
    }

    // Evitar cerrar mientras imprime o cancela
    private void Window_Closing(
        object? sender,
        CancelEventArgs e)
    {
        if (_viewModel.IsBusy)
            e.Cancel = true;
    }
}