using System.ComponentModel;
using System.Windows;
using PurchaseTicket.Desktop.ViewModels.Startup;

namespace PurchaseTicket.Desktop.Views.Startup;

// Primera ventana al abrir la app; la principal solo se crea si la base esta lista
public partial class StartupWindow : Window
{
    // Campos

    private readonly StartupViewModel _viewModel;

    // El cierre lo pidio el arranque, no la X de la ventana
    private bool _isFinishing;

    // Constructor

    public StartupWindow(StartupViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        _viewModel.Finished += OnFinished;

        Loaded += async (_, _) => await _viewModel.RunAsync();
        Closed += (_, _) => _viewModel.Stop();
    }

    // Propiedades

    // true si la base quedo lista; false si el operador salio o cerro la ventana
    public bool IsDatabaseReady { get; private set; }

    // Metodos privados

    private void OnFinished(object? sender, bool isReady)
    {
        IsDatabaseReady = isReady;
        _isFinishing = true;

        Close();
    }

    // A mitad de una actualizacion no se cierra: la base quedaria a medias
    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (!_isFinishing && _viewModel.Step is StartupStep.Migrating or StartupStep.BackingUp)
            e.Cancel = true;
    }
}
