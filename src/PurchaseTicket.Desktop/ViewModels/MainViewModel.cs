using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.Home;
using PurchaseTicket.Desktop.ViewModels.Suppliers;

namespace PurchaseTicket.Desktop.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly HomeViewModel _homeViewModel;
    private readonly SuppliersViewModel _suppliersViewModel;
    private object? _currentViewModel;
    private bool _isSidebarExpanded = true;

    public MainViewModel(HomeViewModel homeViewModel, SuppliersViewModel suppliersViewModel)
    {
        _homeViewModel = homeViewModel;
        _suppliersViewModel = suppliersViewModel;

        ShowHomeCommand = new RelayCommand(ShowHome);
        ShowSuppliersCommand = new AsyncRelayCommand(ShowSuppliersAsync);
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
        }
    }
    public bool IsHomeSelected => ReferenceEquals(CurrentViewModel, _homeViewModel);
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

    private void ShowHome()
    {
        CurrentViewModel = _homeViewModel;
    }

    private async Task ShowSuppliersAsync()
    {
        CurrentViewModel = _suppliersViewModel;

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
}
