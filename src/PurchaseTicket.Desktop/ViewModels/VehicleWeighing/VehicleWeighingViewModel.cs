using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Get;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Services;

namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

public class VehicleWeighingViewModel : INotifyPropertyChanged
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWeighingDialogService _weighingDialogService;

    private string _searchText = string.Empty;
    private string _queryErrorMessage = string.Empty;
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private bool _isLoading;
    private PurchaseTicketDto? _selectedTicket;

    public VehicleWeighingViewModel(
        IServiceScopeFactory scopeFactory,
        IWeighingDialogService weighingDialogService)
    {
        _scopeFactory = scopeFactory;
        _weighingDialogService = weighingDialogService;

        SearchTicketsCommand = new AsyncRelayCommand(
            SearchTicketsAsync,
            CanSearchTickets);

        NextPageCommand = new AsyncRelayCommand(
            NextPageAsync,
            CanGoToNextPage);

        PreviousPageCommand = new AsyncRelayCommand(
            PreviousPageAsync,
            CanGoToPreviousPage);

        ChangePageSizeCommand = new AsyncRelayCommand(
            ChangePageSizeAsync,
            CanChangePageSize);

        NewWeighingCommand = new AsyncRelayCommand(
            NewWeighingAsync,
            CanCreateWeighing);
    }

    // Coleccion de tickets pendientes de tara
    public ObservableCollection<PurchaseTicketDto> Tickets { get; } = new();

    public int[] PageSizeOptions { get; } = [10, 20, 30, 50, 100];

    public AsyncRelayCommand SearchTicketsCommand { get; }
    public AsyncRelayCommand NextPageCommand { get; }
    public AsyncRelayCommand PreviousPageCommand { get; }
    public AsyncRelayCommand ChangePageSizeCommand { get; }
    public AsyncRelayCommand NewWeighingCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;

            _searchText = value;
            OnPropertyChanged(nameof(SearchText));
        }
    }

    public string QueryErrorMessage
    {
        get => _queryErrorMessage;
        private set
        {
            if (_queryErrorMessage == value)
                return;

            _queryErrorMessage = value;
            OnPropertyChanged(nameof(QueryErrorMessage));
        }
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (_currentPage == value)
                return;

            _currentPage = value;
            OnPropertyChanged(nameof(CurrentPage));
            NotifyPaginationCommands();
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (_pageSize == value || !PageSizeOptions.Contains(value))
                return;

            _pageSize = value;
            OnPropertyChanged(nameof(PageSize));
            OnPropertyChanged(nameof(TotalPages));
            NotifyPaginationCommands();
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            if (_totalCount == value)
                return;

            _totalCount = value;
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(TotalPages));
            NotifyPaginationCommands();
        }
    }

    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading == value)
                return;

            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
            NotifyCommandsCanExecuteChanged();
        }
    }

    public PurchaseTicketDto? SelectedTicket
    {
        get => _selectedTicket;
        set
        {
            if (_selectedTicket == value)
                return;

            _selectedTicket = value;
            OnPropertyChanged(nameof(SelectedTicket));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    // Cargar tickets pendientes
    public async Task LoadTicketsAsync()
    {
        await LoadPageAsync(CurrentPage);
    }

    private bool CanSearchTickets() => !IsLoading;

    private bool CanGoToNextPage() =>
        !IsLoading && CurrentPage < TotalPages;

    private bool CanGoToPreviousPage() =>
        !IsLoading && CurrentPage > 1;

    private bool CanChangePageSize() => !IsLoading;

    private async Task SearchTicketsAsync()
    {
        await LoadPageAsync(1);
    }

    private async Task NextPageAsync()
    {
        await LoadPageAsync(CurrentPage + 1);
    }

    private async Task PreviousPageAsync()
    {
        await LoadPageAsync(CurrentPage - 1);
    }

    private async Task ChangePageSizeAsync()
    {
        await LoadPageAsync(1);
    }

    // Consultar tickets pendientes con paginacion
    private async Task<bool> LoadPageAsync(int page)
    {
        if (IsLoading)
            return false;

        IsLoading = true;
        QueryErrorMessage = string.Empty;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var getTickets = scope.ServiceProvider
                .GetRequiredService<GetPurchaseTickets>();

            var query = new GetPurchaseTicketsQuery(
                Page: page,
                PageSize: PageSize,
                Search: string.IsNullOrWhiteSpace(SearchText)
                    ? null
                    : SearchText.Trim(),
                Status: TicketStatus.WeighingPending);

            var result = await getTickets.ExecuteAsync(query);

            var totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    (double)result.TotalCount / PageSize));

            // Ajustar pagina si disminuye el total
            if (page > totalPages)
            {
                page = totalPages;
                query = query with { Page = page };

                result = await getTickets.ExecuteAsync(query);
            }

            SelectedTicket = null;
            Tickets.Clear();

            foreach (var ticket in result.Items)
                Tickets.Add(ticket);

            TotalCount = result.TotalCount;
            CurrentPage = page;

            return true;
        }
        catch (Exception ex)
        {
            QueryErrorMessage =
                "No fue posible cargar los pesajes pendientes.";

            Console.Error.WriteLine(ex);
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void NotifyPaginationCommands()
    {
        NextPageCommand?.NotifyCanExecuteChanged();
        PreviousPageCommand?.NotifyCanExecuteChanged();
    }

    // Validar apertura del formulario
    private bool CanCreateWeighing()
    {
        return !IsLoading;
    }

    // Abrir el formulario de nuevo pesaje
    private async Task NewWeighingAsync()
    {
        if (!CanCreateWeighing())
            return;

        var saved = _weighingDialogService.ShowCreateDialog();

        if (!saved)
            return;

        await LoadPageAsync(1);
    }

    private void NotifyCommandsCanExecuteChanged()
    {
        SearchTicketsCommand?.NotifyCanExecuteChanged();
        NextPageCommand?.NotifyCanExecuteChanged();
        PreviousPageCommand?.NotifyCanExecuteChanged();
        ChangePageSizeCommand?.NotifyCanExecuteChanged();
    }
}
