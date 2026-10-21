using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Desktop.Commands;

namespace PurchaseTicket.Desktop.ViewModels.Base;

// Base de las pantallas con tabla paginada: busqueda, paginacion, carga y errores.
// Cada pantalla solo define como consultar sus registros y sus acciones propias.
public abstract class PagedListViewModel<TItem> : ObservableObject
    where TItem : class
{
    // Campos

    private string _searchText = string.Empty;
    private string _queryErrorMessage = string.Empty;
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private bool _isLoading;
    private bool _isBusy;
    private TItem? _selectedItem;
    private string? _appliedSearch;

    // Constructor

    protected PagedListViewModel(
        IServiceScopeFactory scopeFactory,
        ILogger logger)
    {
        ScopeFactory = scopeFactory;
        Logger = logger;

        SearchCommand = new AsyncRelayCommand(
            () => LoadPageAsync(1),
            CanInteract);

        ChangePageSizeCommand = new AsyncRelayCommand(
            () => LoadPageAsync(1),
            CanInteract);

        NextPageCommand = new AsyncRelayCommand(
            () => LoadPageAsync(CurrentPage + 1),
            CanGoToNextPage);

        PreviousPageCommand = new AsyncRelayCommand(
            () => LoadPageAsync(CurrentPage - 1),
            CanGoToPreviousPage);
    }

    // Propiedades

    public ObservableCollection<TItem> Items { get; } = new();

    public int[] PageSizeOptions { get; } = [10, 20, 30, 50, 100];

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string QueryErrorMessage
    {
        get => _queryErrorMessage;
        protected set
        {
            if (SetProperty(ref _queryErrorMessage, value))
                OnPropertyChanged(nameof(IsEmpty));
        }
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetProperty(ref _currentPage, value))
                NotifyPaginationCommands();
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (!PageSizeOptions.Contains(value))
                return;

            if (!SetProperty(ref _pageSize, value))
                return;

            OnPropertyChanged(nameof(TotalPages));
            NotifyPaginationCommands();
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            if (!SetProperty(ref _totalCount, value))
                return;

            OnPropertyChanged(nameof(TotalPages));
            NotifyPaginationCommands();
        }
    }

    public int TotalPages => CalculateTotalPages(TotalCount);

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (!SetProperty(ref _isLoading, value))
                return;

            NotifyCommandsCanExecuteChanged();

            // Al terminar la carga se decide si mostrar el mensaje de vacio
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(EmptyMessage));
        }
    }

    // Operacion distinta a la consulta en curso, como cambiar el estado de un registro
    public bool IsBusy
    {
        get => _isBusy;
        protected set
        {
            if (SetProperty(ref _isBusy, value))
                NotifyCommandsCanExecuteChanged();
        }
    }

    public TItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    // Sin registros, sin carga en curso y sin error que ya lo explique
    public bool IsEmpty =>
        !IsLoading
        && Items.Count == 0
        && string.IsNullOrEmpty(QueryErrorMessage);

    public string EmptyMessage =>
        HasAppliedFilters
            ? NoResultsMessage
            : EmptyListMessage;

    // Comandos

    public AsyncRelayCommand SearchCommand { get; }

    public AsyncRelayCommand ChangePageSizeCommand { get; }

    public AsyncRelayCommand NextPageCommand { get; }

    public AsyncRelayCommand PreviousPageCommand { get; }

    // Propiedades protegidas

    // La busqueda aplicada cuenta como filtro; cada pantalla agrega los suyos
    protected bool HasAppliedFilters =>
        _appliedSearch is not null || HasExtraFilters;

    protected virtual bool HasExtraFilters => false;

    protected IServiceScopeFactory ScopeFactory { get; }

    // Logger de la pantalla concreta: el registro indica que lista fallo
    protected ILogger Logger { get; }

    // Mensaje que se muestra si la consulta falla
    protected abstract string LoadErrorMessage { get; }

    // Mensaje cuando no existe ningun registro
    protected abstract string EmptyListMessage { get; }

    // Mensaje cuando los filtros no encontraron registros
    protected abstract string NoResultsMessage { get; }

    // Metodos publicos

    // Carga o recarga la pagina actual; se llama al entrar a la pantalla
    public Task LoadAsync()
    {
        return LoadPageAsync(CurrentPage);
    }

    // Metodos protegidos

    // Cada pantalla consulta sus registros con sus propios filtros
    protected abstract Task<PagedResult<TItem>> FetchPageAsync(
        IServiceProvider services,
        int page,
        int pageSize,
        string? search);

    // Ninguna accion se permite mientras se consulta o se ejecuta otra operacion
    protected bool CanInteract()
    {
        return !IsLoading && !IsBusy;
    }

    // Las pantallas que agregan comandos propios lo extienden
    protected virtual void NotifyCommandsCanExecuteChanged()
    {
        SearchCommand?.NotifyCanExecuteChanged();
        ChangePageSizeCommand?.NotifyCanExecuteChanged();
        NotifyPaginationCommands();
    }

    protected async Task<bool> LoadPageAsync(int page)
    {
        if (IsLoading)
            return false;

        IsLoading = true;
        QueryErrorMessage = string.Empty;

        try
        {
            await using var scope = ScopeFactory.CreateAsyncScope();

            string? search = string.IsNullOrWhiteSpace(SearchText)
                ? null
                : SearchText.Trim();

            _appliedSearch = search;

            page = Math.Max(1, page);

            var result = await FetchPageAsync(
                scope.ServiceProvider,
                page,
                PageSize,
                search);

            // La pagina pedida puede ya no existir, por ejemplo al
            // desactivar el ultimo registro de la ultima pagina filtrada
            int lastPage = CalculateTotalPages(result.TotalCount);

            if (page > lastPage)
            {
                page = lastPage;

                result = await FetchPageAsync(
                    scope.ServiceProvider,
                    page,
                    PageSize,
                    search);
            }

            SelectedItem = null;
            Items.Clear();

            foreach (var item in result.Items)
                Items.Add(item);

            TotalCount = result.TotalCount;
            CurrentPage = page;

            return true;
        }
        catch (Exception ex)
        {
            QueryErrorMessage = LoadErrorMessage;

            Logger.LogError(
                ex,
                "No fue posible cargar la página {Page} del listado.",
                page);

            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Recarga despues de una accion que ya se completo.
    // Si la recarga falla, aclara que la accion si se guardo.
    protected async Task ReloadAfterActionAsync(
        int page,
        string failureMessage)
    {
        bool loaded = await LoadPageAsync(page);

        if (!loaded)
            QueryErrorMessage = failureMessage;
    }

    // Metodos privados

    private int CalculateTotalPages(int totalCount)
    {
        return Math.Max(
            1,
            (int)Math.Ceiling((double)totalCount / PageSize));
    }

    private bool CanGoToNextPage()
    {
        return CanInteract() && CurrentPage < TotalPages;
    }

    private bool CanGoToPreviousPage()
    {
        return CanInteract() && CurrentPage > 1;
    }

    private void NotifyPaginationCommands()
    {
        NextPageCommand?.NotifyCanExecuteChanged();
        PreviousPageCommand?.NotifyCanExecuteChanged();
    }
}