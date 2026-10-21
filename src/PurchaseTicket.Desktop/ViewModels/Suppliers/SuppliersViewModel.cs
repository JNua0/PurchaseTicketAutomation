using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.UseCases.Suppliers.Activate;
using PurchaseTicket.Application.UseCases.Suppliers.Deactivate;
using PurchaseTicket.Application.UseCases.Suppliers.Get;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Domain.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PurchaseTicket.Desktop.ViewModels.Suppliers;

public class SuppliersViewModel : INotifyPropertyChanged
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISupplierDialogService _supplierDialogService;
    private readonly IConfirmationDialogService _confirmationDialogService;

    private string _queryErrorMessage = string.Empty;
    private string _searchText = string.Empty;
    private bool? _isActiveFilter;
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private bool _isLoading;
    private SupplierDto? _selectedSupplier;
    public int[] PageSizeOptions { get; } = [10, 20, 30, 50, 100];
    private bool _isChangingStatus;

    public SuppliersViewModel(
        IServiceScopeFactory scopeFactory,
        ISupplierDialogService supplierDialogService,
        IConfirmationDialogService confirmationDialogService)
    {
        _scopeFactory = scopeFactory;
        _supplierDialogService = supplierDialogService;
        _confirmationDialogService = confirmationDialogService;

        // Comandos generales
        SearchSuppliersCommand = new AsyncRelayCommand(
            SearchSuppliersAsync,
            CanSearchSuppliers);

        NextPageCommand = new AsyncRelayCommand(
            NextPageAsync,
            CanGoToNextPage);

        PreviousPageCommand = new AsyncRelayCommand(
            PreviousPageAsync,
            CanGoToPreviousPage);

        NewSupplierCommand = new AsyncRelayCommand(
            NewSupplierAsync,
            CanOpenSupplierForm);

        // Comandos que reciben un proveedor
        EditSupplierCommand = new AsyncRelayCommand<SupplierDto>(
            EditSupplierAsync,
            CanEditSupplier);

        ChangeSupplierStatusCommand = new AsyncRelayCommand<SupplierDto>(
            ChangeSupplierStatusAsync,
            CanChangeSupplierStatus);

        ChangePageSizeCommand = new AsyncRelayCommand(
            ChangePageSizeAsync,
            CanChangePageSize);
    }

    // Coleccion de proveedores
    public ObservableCollection<SupplierDto> Suppliers { get; } = new();

    // Comandos generales
    public AsyncRelayCommand SearchSuppliersCommand { get; }
    public AsyncRelayCommand NextPageCommand { get; }
    public AsyncRelayCommand PreviousPageCommand { get; }
    public AsyncRelayCommand NewSupplierCommand { get; }
    public AsyncRelayCommand ChangePageSizeCommand { get; }

    // Comandos para acciones individuales
    public AsyncRelayCommand<SupplierDto> EditSupplierCommand { get; }
    public AsyncRelayCommand<SupplierDto> ChangeSupplierStatusCommand { get; }

    // Mensaje de error de consultas
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

    // Texto de busqueda
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

    // Filtro de proveedores activos o inactivos
    public bool? IsActiveFilter
    {
        get => _isActiveFilter;
        set
        {
            if (_isActiveFilter == value)
                return;

            _isActiveFilter = value;
            OnPropertyChanged(nameof(IsActiveFilter));

            // Elegir un filtro es una accion deliberada: recargar de inmediato
            SearchSuppliersCommand.Execute(null);
        }
    }

    // Pagina actual
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

    // Cantidad de registros por pagina
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


    // Cantidad total de proveedores
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

    // Cantidad total de paginas
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

    // Estado de carga
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

    // Proveedor seleccionado
    public SupplierDto? SelectedSupplier
    {
        get => _selectedSupplier;
        set
        {
            if (_selectedSupplier == value)
                return;

            _selectedSupplier = value;
            OnPropertyChanged(nameof(SelectedSupplier));
        }
    }

    // Estado de actualizacion
    public bool IsChangingStatus
    {
        get => _isChangingStatus;
        private set
        {
            if (_isChangingStatus == value)
                return;

            _isChangingStatus = value;
            OnPropertyChanged(nameof(IsChangingStatus));

            NotifyCommandsCanExecuteChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    // Cargar proveedores
    public async Task LoadSuppliersAsync()
    {
        await LoadPageAsync(CurrentPage);
    }

    // Validar navegacion a la siguiente pagina
    private bool CanGoToNextPage()
    {
        return !IsLoading
            && !IsChangingStatus
            && CurrentPage < TotalPages;
    }

    // Validar navegacion a la pagina anterior
    private bool CanGoToPreviousPage()
    {
        return !IsLoading
            && !IsChangingStatus
            && CurrentPage > 1;
    }

    // Navegar a la siguiente pagina
    private async Task NextPageAsync()
    {
        await GoToPageAsync(CurrentPage + 1);
    }

    // Navegar a la pagina anterior
    private async Task PreviousPageAsync()
    {
        await GoToPageAsync(CurrentPage - 1);
    }

    // Notificar cambios en la paginacion
    private void NotifyPaginationCommands()
    {
        NextPageCommand?.NotifyCanExecuteChanged();
        PreviousPageCommand?.NotifyCanExecuteChanged();
    }

    // Buscar proveedores
    private async Task SearchSuppliersAsync()
    {
        await LoadPageAsync(1);
    }

    // Actualizar los registros mostrados
    private async Task ChangePageSizeAsync()
    {
        await LoadPageAsync(1);
    }

    // Navegar a una pagina especifica
    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages)
            return;

        await LoadPageAsync(page);
    }

    // Validar disponibilidad de busqueda
    private bool CanSearchSuppliers()
    {
        return !IsLoading && !IsChangingStatus;
    }

    // Consultar proveedores con paginacion
    private async Task<bool> LoadPageAsync(int page)
    {
        if (IsLoading)
            return false;

        IsLoading = true;
        QueryErrorMessage = string.Empty;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var getSuppliers = scope.ServiceProvider
                .GetRequiredService<GetSuppliers>();

            var query = new GetSuppliersQuery(
                Page: page,
                PageSize: PageSize,
                Search: string.IsNullOrWhiteSpace(SearchText)
                    ? null
                    : SearchText.Trim(),
                IsActive: IsActiveFilter);

            var result = await getSuppliers.ExecuteAsync(query);

            var totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    (double)result.TotalCount / PageSize));

            if (page > totalPages)
            {
                page = totalPages;

                query = query with
                {
                    Page = page
                };

                result = await getSuppliers.ExecuteAsync(query);
            }

            SelectedSupplier = null;
            Suppliers.Clear();

            foreach (var supplier in result.Items)
            {
                Suppliers.Add(supplier);
            }

            TotalCount = result.TotalCount;
            CurrentPage = page;

            return true;
        }
        catch (Exception ex)
        {
            QueryErrorMessage =
                "No fue posible cargar los proveedores.";

            Console.Error.WriteLine(ex);

            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Validar apertura del formulario
    private bool CanOpenSupplierForm()
    {
        return !IsLoading && !IsChangingStatus;
    }

    // Registrar un proveedor
    private async Task NewSupplierAsync()
    {
        if (!CanOpenSupplierForm())
            return;

        var saved = _supplierDialogService.ShowCreateDialog();

        if (!saved)
            return;

        var loaded = await LoadPageAsync(1);

        if (!loaded)
        {
            QueryErrorMessage =
                "El proveedor se registró correctamente, " +
                "pero no fue posible actualizar el listado.";
        }
    }

    // Validar edicion de un proveedor
    private bool CanEditSupplier(SupplierDto supplier)
    {
        return supplier is not null
            && !IsLoading
            && !IsChangingStatus;
    }

    // Editar el proveedor recibido
    private async Task EditSupplierAsync(SupplierDto supplier)
    {
        if (!CanEditSupplier(supplier))
            return;

        var saved = _supplierDialogService.ShowEditDialog(
            supplier.Id,
            supplier.Name,
            supplier.PhoneNumber);

        if (!saved)
            return;

        var loaded = await LoadPageAsync(CurrentPage);

        if (!loaded)
        {
            QueryErrorMessage =
                "El proveedor se actualizó correctamente, " +
                "pero no fue posible actualizar el listado.";
        }
    }

    // Validar cambio de estado
    private bool CanChangeSupplierStatus(SupplierDto supplier)
    {
        return supplier is not null
            && !IsLoading
            && !IsChangingStatus;
    }

    // Activar o desactivar el proveedor recibido
    private async Task ChangeSupplierStatusAsync(SupplierDto supplier)
    {
        if (!CanChangeSupplierStatus(supplier))
            return;

        var action = supplier.IsActive
            ? "desactivar"
            : "activar";

        var confirmed = _confirmationDialogService.Confirm(
            "Confirmar cambio de estado",
            $"¿Estás seguro de que deseas {action} al proveedor " +
            $"\"{supplier.Name}\"?");

        if (!confirmed)
            return;

        IsChangingStatus = true;
        QueryErrorMessage = string.Empty;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            if (supplier.IsActive)
            {
                var deactivateSupplier = scope.ServiceProvider
                    .GetRequiredService<DeactivateSupplier>();

                await deactivateSupplier.ExecuteAsync(supplier.Id);
            }
            else
            {
                var activateSupplier = scope.ServiceProvider
                    .GetRequiredService<ActivateSupplier>();

                await activateSupplier.ExecuteAsync(supplier.Id);
            }

            var loaded = await LoadPageAsync(CurrentPage);

            if (!loaded)
            {
                QueryErrorMessage =
                    "El estado del proveedor se actualizó correctamente, " +
                    "pero no fue posible actualizar el listado.";
            }
        }
        catch (Exception ex)
        {
            QueryErrorMessage =
                "No fue posible cambiar el estado del proveedor.";

            Console.Error.WriteLine(ex);
        }
        finally
        {
            IsChangingStatus = false;
        }
    }

    // Validar cambio de registros por pagina
    private bool CanChangePageSize()
    {
        return !IsLoading && !IsChangingStatus;
    }

    // Notificar disponibilidad de comandos
    private void NotifyCommandsCanExecuteChanged()
    {
        SearchSuppliersCommand?.NotifyCanExecuteChanged();
        NextPageCommand?.NotifyCanExecuteChanged();
        PreviousPageCommand?.NotifyCanExecuteChanged();
        NewSupplierCommand?.NotifyCanExecuteChanged();
        EditSupplierCommand?.NotifyCanExecuteChanged();
        ChangeSupplierStatusCommand?.NotifyCanExecuteChanged();
        ChangePageSizeCommand?.NotifyCanExecuteChanged();
    }
}