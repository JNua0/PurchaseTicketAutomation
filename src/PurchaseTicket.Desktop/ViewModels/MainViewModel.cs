using System.Windows.Input;
using System.Windows.Threading;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Configuration;
using PurchaseTicket.Desktop.ViewModels.Amounts;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Desktop.ViewModels.Home;
using PurchaseTicket.Desktop.ViewModels.Materials;
using PurchaseTicket.Desktop.ViewModels.Navigation;
using PurchaseTicket.Desktop.ViewModels.Settings;
using PurchaseTicket.Desktop.ViewModels.Suppliers;
using PurchaseTicket.Desktop.ViewModels.Tickets;
using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

namespace PurchaseTicket.Desktop.ViewModels;

// Ventana principal: el menu lateral y la pantalla que se muestra. Cada
// opcion del menu es un NavigationItem; navegar es lo mismo para todas
public class MainViewModel : ObservableObject
{
    // Campos

    // Clave en appsettings.local.json; el menu se recuerda entre sesiones
    private const string SidebarSection = "Sidebar";
    private const string SidebarExpandedKey = "IsExpanded";

    private readonly ApplicationSettingsService _settings;
    private readonly NavigationItem _home;
    private readonly NavigationItem _vehicleWeighing;
    private readonly NavigationItem _ticketAmounts;
    private readonly NavigationItem _settingsItem;
    private readonly IReadOnlyList<NavigationItem> _allItems;
    private object? _currentViewModel;
    private bool _isSidebarExpanded;

    // Constructor

    public MainViewModel(
        HomeViewModel homeViewModel,
        SuppliersViewModel suppliersViewModel,
        MaterialsViewModel materialsViewModel,
        VehicleWeighingViewModel vehicleWeighingViewModel,
        TicketAmountsViewModel ticketAmountsViewModel,
        TicketsViewModel ticketsViewModel,
        SettingsViewModel settingsViewModel,
        ApplicationSettingsService settings)
    {
        _settings = settings;

        // Inicio se recarga sola al mostrarse; las demas cargan al entrar
        _home = new NavigationItem("Inicio", "HomeIcon", homeViewModel);
        _vehicleWeighing = new NavigationItem("Pesaje de vehículos", "TruckIcon", vehicleWeighingViewModel, vehicleWeighingViewModel.LoadAsync);
        _ticketAmounts = new NavigationItem("Importes de tickets", "TicketAmountIcon", ticketAmountsViewModel, ticketAmountsViewModel.LoadAsync);

        var tickets = new NavigationItem("Tickets", "TicketsIcon", ticketsViewModel, ticketsViewModel.InitializeAsync);
        var suppliers = new NavigationItem("Proveedores", "SuppliersIcon", suppliersViewModel, suppliersViewModel.LoadAsync);
        var materials = new NavigationItem("Materiales", "MaterialsIcon", materialsViewModel, materialsViewModel.LoadAsync);

        _settingsItem = new NavigationItem("Configuración", "SettingsIcon", settingsViewModel, settingsViewModel.LoadAsync);

        Sections =
        [
            new NavigationSection("OPERACIÓN", [_home, _vehicleWeighing, _ticketAmounts, tickets]),
            new NavigationSection("CATÁLOGOS", [suppliers, materials])
        ];

        _allItems = [.. Sections.SelectMany(section => section.Items), _settingsItem];

        NavigateCommand = new AsyncRelayCommand<NavigationItem>(NavigateAsync, item => item is not null);
        ToggleSidebarCommand = new RelayCommand(ToggleSidebar);

        // Accesos directos que usa Inicio
        ShowVehicleWeighingCommand = new AsyncRelayCommand(() => NavigateAsync(_vehicleWeighing));
        ShowTicketAmountsCommand = new AsyncRelayCommand(() => NavigateAsync(_ticketAmounts));
        ShowSettingsCommand = new AsyncRelayCommand(() => NavigateAsync(_settingsItem));

        _isSidebarExpanded = !bool.TryParse(
            _settings.GetValue($"{SidebarSection}:{SidebarExpandedKey}"),
            out bool isExpanded) || isExpanded;

        Select(_home);
    }

    // Propiedades

    public IReadOnlyList<NavigationSection> Sections { get; }

    // Configuracion va aparte, anclada al fondo del menu
    public NavigationItem SettingsItem => _settingsItem;

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public bool IsSidebarExpanded
    {
        get => _isSidebarExpanded;
        private set => SetProperty(ref _isSidebarExpanded, value);
    }

    // Comandos

    public AsyncRelayCommand<NavigationItem> NavigateCommand { get; }

    public ICommand ToggleSidebarCommand { get; }

    public ICommand ShowVehicleWeighingCommand { get; }

    public ICommand ShowTicketAmountsCommand { get; }

    public ICommand ShowSettingsCommand { get; }

    // Metodos privados

    private void Select(NavigationItem item)
    {
        foreach (var other in _allItems)
            other.IsSelected = ReferenceEquals(other, item);

        CurrentViewModel = item.ViewModel;
    }

    // Se recarga aunque ya sea la pantalla actual: asi un clic en el menu
    // siempre trae los datos al dia
    private async Task NavigateAsync(NavigationItem item)
    {
        Select(item);

        if (item.OnEnter is null)
            return;

        // Primero se dibuja la pantalla, despues se cargan sus datos
        await System.Windows.Application.Current.Dispatcher.InvokeAsync(
            () => { },
            DispatcherPriority.ApplicationIdle);

        await item.OnEnter();
    }

    private void ToggleSidebar()
    {
        IsSidebarExpanded = !IsSidebarExpanded;

        try
        {
            _settings.SaveValue(
                SidebarSection,
                SidebarExpandedKey,
                IsSidebarExpanded.ToString());
        }
        catch (Exception)
        {
            // No poder recordarlo no impide usar el menu
        }
    }
}
