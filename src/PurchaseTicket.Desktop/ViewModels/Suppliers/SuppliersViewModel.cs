using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Suppliers.Activate;
using PurchaseTicket.Application.UseCases.Suppliers.Deactivate;
using PurchaseTicket.Application.UseCases.Suppliers.Get;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Base;

namespace PurchaseTicket.Desktop.ViewModels.Suppliers;

// Listado de proveedores: busqueda y paginacion vienen de la clase base
public class SuppliersViewModel : PagedListViewModel<SupplierDto>
{
    // Campos

    private readonly ISupplierDialogService _supplierDialogService;
    private readonly IConfirmationDialogService _confirmationDialogService;

    private bool? _isActiveFilter;

    // Constructor

    public SuppliersViewModel(
        IServiceScopeFactory scopeFactory,
        ISupplierDialogService supplierDialogService,
        IConfirmationDialogService confirmationDialogService)
        : base(scopeFactory)
    {
        _supplierDialogService = supplierDialogService;
        _confirmationDialogService = confirmationDialogService;

        NewSupplierCommand = new AsyncRelayCommand(
            NewSupplierAsync,
            CanInteract);

        EditSupplierCommand = new AsyncRelayCommand<SupplierDto>(
            EditSupplierAsync,
            CanActOn);

        ChangeSupplierStatusCommand = new AsyncRelayCommand<SupplierDto>(
            ChangeSupplierStatusAsync,
            CanActOn);
    }

    // Propiedades

    // Null muestra todos; true solo activos; false solo inactivos
    public bool? IsActiveFilter
    {
        get => _isActiveFilter;
        set
        {
            // Elegir un filtro es una accion deliberada: recargar de inmediato
            if (SetProperty(ref _isActiveFilter, value))
                SearchCommand.Execute(null);
        }
    }

    protected override string LoadErrorMessage =>
        "No fue posible cargar los proveedores.";

    protected override string EmptyListMessage =>
    "Aún no hay proveedores registrados.";

    protected override string NoResultsMessage =>
        "No se encontraron proveedores con los filtros aplicados.";

    protected override bool HasExtraFilters =>
        IsActiveFilter is not null;

    // Comandos

    public AsyncRelayCommand NewSupplierCommand { get; }

    public AsyncRelayCommand<SupplierDto> EditSupplierCommand { get; }

    public AsyncRelayCommand<SupplierDto> ChangeSupplierStatusCommand { get; }

    // Metodos protegidos

    protected override Task<PagedResult<SupplierDto>> FetchPageAsync(
        IServiceProvider services,
        int page,
        int pageSize,
        string? search)
    {
        var getSuppliers =
            services.GetRequiredService<GetSuppliers>();

        return getSuppliers.ExecuteAsync(
            new GetSuppliersQuery(
                Page: page,
                PageSize: pageSize,
                Search: search,
                IsActive: IsActiveFilter));
    }

    protected override void NotifyCommandsCanExecuteChanged()
    {
        base.NotifyCommandsCanExecuteChanged();

        NewSupplierCommand?.NotifyCanExecuteChanged();
        EditSupplierCommand?.NotifyCanExecuteChanged();
        ChangeSupplierStatusCommand?.NotifyCanExecuteChanged();
    }

    // Metodos privados

    private bool CanActOn(SupplierDto supplier)
    {
        return supplier is not null && CanInteract();
    }

    private async Task NewSupplierAsync()
    {
        if (!_supplierDialogService.ShowCreateDialog())
            return;

        await ReloadAfterActionAsync(
            page: 1,
            "El proveedor se registró correctamente, " +
            "pero no fue posible actualizar el listado.");
    }

    private async Task EditSupplierAsync(SupplierDto supplier)
    {
        bool saved = _supplierDialogService.ShowEditDialog(
            supplier.Id,
            supplier.Name,
            supplier.PhoneNumber);

        if (!saved)
            return;

        await ReloadAfterActionAsync(
            CurrentPage,
            "El proveedor se actualizó correctamente, " +
            "pero no fue posible actualizar el listado.");
    }

    private async Task ChangeSupplierStatusAsync(SupplierDto supplier)
    {
        string action = supplier.IsActive
            ? "desactivar"
            : "activar";

        bool confirmed = _confirmationDialogService.Confirm(
            "Confirmar cambio de estado",
            $"¿Estás seguro de que deseas {action} al proveedor " +
            $"\"{supplier.Name}\"?");

        if (!confirmed)
            return;

        IsBusy = true;
        QueryErrorMessage = string.Empty;

        try
        {
            await using var scope = ScopeFactory.CreateAsyncScope();

            if (supplier.IsActive)
            {
                await scope.ServiceProvider
                    .GetRequiredService<DeactivateSupplier>()
                    .ExecuteAsync(supplier.Id);
            }
            else
            {
                await scope.ServiceProvider
                    .GetRequiredService<ActivateSupplier>()
                    .ExecuteAsync(supplier.Id);
            }
        }
        catch (Exception ex)
        {
            QueryErrorMessage =
                "No fue posible cambiar el estado del proveedor.";

            Console.Error.WriteLine(ex);

            return;
        }
        finally
        {
            IsBusy = false;
        }

        await ReloadAfterActionAsync(
            CurrentPage,
            "El estado del proveedor se actualizó correctamente, " +
            "pero no fue posible actualizar el listado.");
    }
}