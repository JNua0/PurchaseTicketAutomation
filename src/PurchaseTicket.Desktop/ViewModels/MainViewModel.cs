using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.Home;
using PurchaseTicket.Desktop.ViewModels.Suppliers;
using PurchaseTicket.Desktop.ViewModels.Materials;
using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;
using PurchaseTicket.Desktop.ViewModels.Settings;

namespace PurchaseTicket.Desktop.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly HomeViewModel _homeViewModel;
    private readonly SuppliersViewModel _suppliersViewModel;
    private readonly MaterialsViewModel _materialsViewModel;
    private readonly VehicleWeighingViewModel _vehicleWeighingViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private object? _currentViewModel;
    private bool _isSidebarExpanded = true;

    public MainViewModel(
        HomeViewModel homeViewModel,
        SuppliersViewModel suppliersViewModel,
        MaterialsViewModel materialsViewModel,
        VehicleWeighingViewModel vehicleWeighingViewModel,
        SettingsViewModel settingsViewModel)
    {
        _homeViewModel = homeViewModel;
        _suppliersViewModel = suppliersViewModel;
        _materialsViewModel = materialsViewModel;
        _vehicleWeighingViewModel = vehicleWeighingViewModel;
        _settingsViewModel = settingsViewModel;

        ShowHomeCommand = new RelayCommand(ShowHome);
        ShowSuppliersCommand = new AsyncRelayCommand(ShowSuppliersAsync);
        ShowMaterialsCommand = new AsyncRelayCommand(ShowMaterialsAsync);
        ShowVehicleWeighingCommand = new AsyncRelayCommand(ShowVehicleWeighingAsync);
        ShowSettingsCommand = new RelayCommand(ShowSettings);
        ToggleSidebarCommand = new RelayCommand(ToggleSidebar);

        CurrentViewModel = _homeViewModel;
    }

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            if (ReferenceEquals(_currentViewModel, value))
                return;

            _currentViewModel = value;

            OnPropertyChanged(nameof(CurrentViewModel));
            OnPropertyChanged(nameof(IsHomeSelected));
            OnPropertyChanged(nameof(IsSuppliersSelected));
            OnPropertyChanged(nameof(IsMaterialsSelected));
            OnPropertyChanged(nameof(IsVehicleWeighingSelected));
        }
    }
    public bool IsHomeSelected => ReferenceEquals(CurrentViewModel, _homeViewModel);
    public bool IsMaterialsSelected => ReferenceEquals(CurrentViewModel, _materialsViewModel);
    public bool IsSuppliersSelected => ReferenceEquals(CurrentViewModel, _suppliersViewModel);
    public bool IsVehicleWeighingSelected => ReferenceEquals(CurrentViewModel, _vehicleWeighingViewModel);
    public bool IsSidebarExpanded
    {
        get => _isSidebarExpanded;
        private set
        {
            if (_isSidebarExpanded == value)
                return;

            _isSidebarExpanded = value;

            OnPropertyChanged(nameof(IsSidebarExpanded));
        }
    }
    public bool IsSettingsSelected => ReferenceEquals(CurrentViewModel, _settingsViewModel);

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand ShowHomeCommand { get; }
    public ICommand ShowSuppliersCommand { get; }
    public ICommand ToggleSidebarCommand { get; }
    public ICommand ShowMaterialsCommand { get; }
    public ICommand ShowVehicleWeighingCommand { get; }
    public ICommand ShowSettingsCommand { get; }

    private void ShowHome()
    {
        CurrentViewModel = _homeViewModel;

        OnPropertyChanged(nameof(IsHomeSelected));
        OnPropertyChanged(nameof(IsSuppliersSelected));
    }

    private async Task ShowSuppliersAsync()
    {
        CurrentViewModel = _suppliersViewModel;

        OnPropertyChanged(nameof(IsHomeSelected));
        OnPropertyChanged(nameof(IsSuppliersSelected));

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(
            () => { },
            DispatcherPriority.ApplicationIdle);

        await _suppliersViewModel.LoadSuppliersAsync();
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    private void ToggleSidebar()
    {
        IsSidebarExpanded = !IsSidebarExpanded;
    }

    // Mostrar el modulo de materiales
    private async Task ShowMaterialsAsync()
    {
        CurrentViewModel = _materialsViewModel;

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(
            () => { },
            DispatcherPriority.ApplicationIdle);

        await _materialsViewModel.LoadMaterialsAsync();
    }

    // Mostrar el modulo de pesaje
    private async Task ShowVehicleWeighingAsync()
    {
        CurrentViewModel = _vehicleWeighingViewModel;

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(
            () => { },
            DispatcherPriority.ApplicationIdle);

        await _vehicleWeighingViewModel.LoadTicketsAsync();
    }

    // Mostrar el modulo de configuracion
    private void ShowSettings()
    {
        CurrentViewModel = _settingsViewModel;
    }
}
