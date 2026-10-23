using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private readonly INotificationService _notifications;
    private bool? _isActiveFilter;

    // Constructor

    public SuppliersViewModel(
        IServiceScopeFactory scopeFactory,
        ISupplierDialogService supplierDialogService,
        IConfirmationDialogService confirmationDialogService,
        INotificationService notifications,
        ILogger<SuppliersViewModel> logger)
        : base(scopeFactory, logger)
    {
        _supplierDialogService = supplierDialogService;
        _confirmationDialogService = confirmationDialogService;
        _notifications = notifications;

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

    // Comandos

    public AsyncRelayCommand NewSupplierCommand { get; }

    public AsyncRelayCommand<SupplierDto> EditSupplierCommand { get; }

    public AsyncRelayCommand<SupplierDto> ChangeSupplierStatusCommand { get; }

    // Propiedades protegidas

    protected override string LoadErrorMessage =>
        "No fue posible cargar los proveedores.";

    protected override string EmptyListMessage =>
    "Aún no hay proveedores registrados.";

    protected override string NoResultsMessage =>
        "No se encontraron proveedores con los filtros aplicados.";

    protected override bool HasExtraFilters =>
        IsActiveFilter is not null;

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
        if (_supplierDialogService.ShowCreateDialog() is not string name)
            return;

        _notifications.ShowSuccess($"Proveedor \"{name}\" registrado.");

        await ReloadAfterActionAsync(
            page: 1,
            "El proveedor se registró correctamente, " +
            "pero no fue posible actualizar el listado.");
    }

    private async Task EditSupplierAsync(SupplierDto supplier)
    {
        // El nombre nuevo, no el de la fila: pudo cambiar al editar
        if (_supplierDialogService.ShowEditDialog(supplier) is not string name)
            return;

        _notifications.ShowSuccess(
            $"Proveedor \"{name}\" actualizado.");

        await ReloadAfterActionAsync(
            CurrentPage,
            "El proveedor se actualizó correctamente, " +
            "pero no fue posible actualizar el listado.");
    }

    private async Task ChangeSupplierStatusAsync(SupplierDto supplier)
    {
        var request = supplier.IsActive
            ? new ConfirmationRequest(
                $"¿Desactivar a {supplier.Name}?",
                "Ya no aparecerá al registrar pesajes nuevos. Sus tickets no cambian " +
                "y puedes activarlo de nuevo cuando quieras.",
                "Desactivar",
                ConfirmationKind.Danger,
                "DeactivateUserIcon")
            : new ConfirmationRequest(
                $"¿Activar a {supplier.Name}?",
                "Volverá a aparecer al registrar pesajes nuevos.",
                "Activar",
                ConfirmationKind.Positive,
                "ActivateUserIcon");

        bool confirmed = _confirmationDialogService.Confirm(request);

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
        catch (NotFoundException ex)
        {
            // La fila estaba desactualizada: se avisa y se recarga para quitarla.
            // Va en un aviso porque la recarga limpia el mensaje de la lista
            _notifications.ShowError(ex.Message);
            IsBusy = false;

            await ReloadAfterActionAsync(
                CurrentPage,
                "No fue posible actualizar el listado.");

            return;
        }
        catch (Exception ex)
        {
            QueryErrorMessage =
                "No fue posible cambiar el estado del proveedor.";

            Logger.LogError(
                ex,
                "No fue posible cambiar el estado del proveedor {SupplierId}.",
                supplier.Id);

            return;
        }
        finally
        {
            IsBusy = false;
        }

        string result = supplier.IsActive
            ? "desactivado"
            : "activado";

        _notifications.ShowSuccess(
            $"Proveedor \"{supplier.Name}\" {result}.");

        await ReloadAfterActionAsync(
            CurrentPage,
            "El estado del proveedor se actualizó correctamente, " +
            "pero no fue posible actualizar el listado.");
    }
}