using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.Home;
using PurchaseTicket.Desktop.ViewModels.Suppliers;
using PurchaseTicket.Desktop.ViewModels.Materials;

namespace PurchaseTicket.Desktop.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly HomeViewModel _homeViewModel;
    private readonly SuppliersViewModel _suppliersViewModel;
    private readonly MaterialsViewModel _materialsViewModel;
    private object? _currentViewModel;
    private bool _isSidebarExpanded = true;

    public MainViewModel(
        HomeViewModel homeViewModel,
        SuppliersViewModel suppliersViewModel,
        MaterialsViewModel materialsViewModel)
    {
        _homeViewModel = homeViewModel;
        _suppliersViewModel = suppliersViewModel;
        _materialsViewModel = materialsViewModel;

        ShowHomeCommand = new RelayCommand(ShowHome);
        ShowSuppliersCommand = new AsyncRelayCommand(ShowSuppliersAsync);
        ShowMaterialsCommand = new AsyncRelayCommand(ShowMaterialsAsync);
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
        }
    }
    public bool IsHomeSelected => ReferenceEquals(CurrentViewModel, _homeViewModel);
    public bool IsMaterialsSelected => ReferenceEquals(CurrentViewModel, _materialsViewModel);
    public bool IsSuppliersSelected => ReferenceEquals(CurrentViewModel, _suppliersViewModel);
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

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand ShowHomeCommand { get; }
    public ICommand ShowSuppliersCommand { get; }
    public ICommand ToggleSidebarCommand { get; }
    public ICommand ShowMaterialsCommand { get; }

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
}
