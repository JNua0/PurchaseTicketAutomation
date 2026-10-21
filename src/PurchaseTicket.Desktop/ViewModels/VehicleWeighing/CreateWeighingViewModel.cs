using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Materials.GetActive;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Correct;
using PurchaseTicket.Application.UseCases.PurchaseTickets.CreateConventional;
using PurchaseTicket.Application.UseCases.PurchaseTickets.CreateSingle;
using PurchaseTicket.Application.UseCases.PurchaseTickets.Print;
using PurchaseTicket.Application.UseCases.Suppliers.GetActive;
using PurchaseTicket.Application.UseCases.Weighing.ReadWeight;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Domain.Exceptions;
using PurchaseTicket.Desktop.Commands;

namespace PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

public class CreateWeighingViewModel : INotifyPropertyChanged
{
    private readonly IServiceScopeFactory _scopeFactory;

    private WeighingType _selectedWeighingType = WeighingType.Conventional;
    private SupplierOptionDto? _selectedSupplier;
    private MaterialOptionDto? _selectedMaterial;
    private string _licensePlate = string.Empty;
    private string _transporter = string.Empty;
    private string _weight = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private string? _ticketNumber;
    private int? _ticketId;
    private bool _isLoadingCatalogs;
    private bool _isReadingWeight;
    private bool _isSaving;
    private bool _isPrinting;
    private bool _isRegistered;
    private bool _isEditing;
    private TicketSnapshot? _savedSnapshot;

    private sealed record TicketSnapshot(
        int SupplierId,
        int MaterialId,
        string? LicensePlate,
        string Transporter,
        decimal Weight);

    public CreateWeighingViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

        ReadWeightCommand = new AsyncRelayCommand(ReadWeightAsync, CanReadWeight);
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        PrintTicketCommand = new AsyncRelayCommand(PrintTicketAsync, CanPrintTicket);
        UpdateCommand = new AsyncRelayCommand(UpdateAsync, CanUpdate);
        EditCommand = new RelayCommand(BeginEdit);
        CancelEditCommand = new RelayCommand(CancelEdit);
    }

    // Catalogos disponibles
    public ObservableCollection<SupplierOptionDto> Suppliers { get; } = new();
    public ObservableCollection<MaterialOptionDto> Materials { get; } = new();

    // Comandos
    public AsyncRelayCommand ReadWeightCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand PrintTicketCommand { get; }
    public AsyncRelayCommand UpdateCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand CancelEditCommand { get; }

    // Estado de la ventana
    public bool ShowForm => !IsRegistered || IsEditing;
    public bool ShowSummary => IsRegistered && !IsEditing;
    public bool IsCreating => !IsRegistered;
    public bool CanEditForm => ShowForm && !IsBusy;
    public bool CanCloseForm => !IsBusy;
    private bool IsBusy => IsLoadingCatalogs || IsReadingWeight || IsSaving || IsPrinting;
    public bool IsRegistered
    {
        get => _isRegistered;
        private set
        {
            if (_isRegistered == value) return;
            _isRegistered = value;
            OnPropertyChanged(nameof(IsRegistered));
            NotifyViewStateChanged();
        }
    }
    public bool IsEditing
    {
        get => _isEditing;
        private set
        {
            if (_isEditing == value) return;
            _isEditing = value;
            OnPropertyChanged(nameof(IsEditing));
            NotifyViewStateChanged();
        }
    }
    public WeighingType SelectedWeighingType
    {
        get => _selectedWeighingType;
        set
        {
            if (_selectedWeighingType == value || IsRegistered || IsBusy) return;
            _selectedWeighingType = value;
            Weight = string.Empty;
            OnPropertyChanged(nameof(SelectedWeighingType));
            OnPropertyChanged(nameof(IsConventional));
            OnPropertyChanged(nameof(IsSingle));
            OnPropertyChanged(nameof(WeightLabel));
        }
    }
    public bool IsConventional
    {
        get => SelectedWeighingType == WeighingType.Conventional;
        set { if (value) SelectedWeighingType = WeighingType.Conventional; }
    }
    public bool IsSingle
    {
        get => SelectedWeighingType == WeighingType.Single;
        set { if (value) SelectedWeighingType = WeighingType.Single; }
    }
    public string WeightLabel => IsConventional ? "Peso bruto (kg)" : "Peso neto (kg)";
    public SupplierOptionDto? SelectedSupplier
    {
        get => _selectedSupplier;
        set
        {
            if (_selectedSupplier == value || (IsRegistered && !IsEditing)) return;
            _selectedSupplier = value;
            OnPropertyChanged(nameof(SelectedSupplier));
            OnPropertyChanged(nameof(SupplierDisplay));
        }
    }
    public MaterialOptionDto? SelectedMaterial
    {
        get => _selectedMaterial;
        set
        {
            if (_selectedMaterial == value || (IsRegistered && !IsEditing)) return;
            _selectedMaterial = value;
            OnPropertyChanged(nameof(SelectedMaterial));
            OnPropertyChanged(nameof(MaterialDisplay));
        }
    }
    public string LicensePlate
    {
        get => _licensePlate;
        set
        {
            if (_licensePlate == value || (IsRegistered && !IsEditing)) return;
            _licensePlate = value;
            OnPropertyChanged(nameof(LicensePlate));
            OnPropertyChanged(nameof(LicensePlateDisplay));
        }
    }
    public string Transporter
    {
        get => _transporter;
        set
        {
            if (_transporter == value || (IsRegistered && !IsEditing)) return;
            _transporter = value;
            OnPropertyChanged(nameof(Transporter));
        }
    }
    // Peso introducido manualmente o recibido del indicador
    public string Weight
    {
        get => _weight;
        set
        {
            if (_weight == value || (IsRegistered && !IsEditing)) return;
            _weight = value;
            OnPropertyChanged(nameof(Weight));
            OnPropertyChanged(nameof(WeightDisplay));
        }
    }
    // Propiedades listas para la vista de resumen
    public string SupplierDisplay => SelectedSupplier?.Name ?? string.Empty;
    public string MaterialDisplay => SelectedMaterial?.Name ?? string.Empty;
    public string LicensePlateDisplay => string.IsNullOrWhiteSpace(LicensePlate) ? "Sin placas" : LicensePlate;
    public string WeighingTypeDisplay => IsConventional ? "Convencional" : "Único";
    public string WeightDisplay => string.IsNullOrWhiteSpace(Weight) ? "" : $"{Weight} kg";
    public string StatusDisplay => IsConventional ? "Pendiente de tara" : "Pendiente de importe";
    public int? TicketId
    {
        get => _ticketId;
        private set
        {
            if (_ticketId == value) return;
            _ticketId = value;
            OnPropertyChanged(nameof(TicketId));
            NotifyCommandsCanExecuteChanged();
        }
    }
    public string? TicketNumber
    {
        get => _ticketNumber;
        private set
        {
            if (_ticketNumber == value) return;
            _ticketNumber = value;
            OnPropertyChanged(nameof(TicketNumber));
            OnPropertyChanged(nameof(TicketNumberDisplay));
        }
    }
    public string TicketNumberDisplay => TicketNumber ?? "Se asignará al registrar";
    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value) return;
            _errorMessage = value;
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }
    public string SuccessMessage
    {
        get => _successMessage;
        private set
        {
            if (_successMessage == value) return;
            _successMessage = value;
            OnPropertyChanged(nameof(SuccessMessage));
        }
    }
    public bool IsLoadingCatalogs
    {
        get => _isLoadingCatalogs;
        private set
        {
            if (_isLoadingCatalogs == value) return;
            _isLoadingCatalogs = value;
            OnPropertyChanged(nameof(IsLoadingCatalogs));
            NotifyViewStateChanged();
        }
    }
    public bool IsReadingWeight
    {
        get => _isReadingWeight;
        private set
        {
            if (_isReadingWeight == value) return;
            _isReadingWeight = value;
            OnPropertyChanged(nameof(IsReadingWeight));
            NotifyViewStateChanged();
        }
    }
    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (_isSaving == value) return;
            _isSaving = value;
            OnPropertyChanged(nameof(IsSaving));
            NotifyViewStateChanged();
        }
    }
    public bool IsPrinting
    {
        get => _isPrinting;
        private set
        {
            if (_isPrinting == value) return;
            _isPrinting = value;
            OnPropertyChanged(nameof(IsPrinting));
            NotifyViewStateChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void NotifyCommandsCanExecuteChanged()
    {
        ReadWeightCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        PrintTicketCommand.NotifyCanExecuteChanged();
        UpdateCommand.NotifyCanExecuteChanged();
    }

    private void NotifyViewStateChanged()
    {
        OnPropertyChanged(nameof(ShowForm));
        OnPropertyChanged(nameof(ShowSummary));
        OnPropertyChanged(nameof(IsCreating));
        OnPropertyChanged(nameof(CanEditForm));
        OnPropertyChanged(nameof(CanCloseForm));
        NotifyCommandsCanExecuteChanged();
    }

    // Preparar un formulario nuevo
    public void Initialize()
    {
        IsEditing = false;
        IsRegistered = false;
        TicketId = null;
        TicketNumber = null;
        _savedSnapshot = null;
        IsPrinting = false;
        SuccessMessage = string.Empty;
        ErrorMessage = string.Empty;

        SelectedWeighingType = WeighingType.Conventional;
        SelectedSupplier = null;
        SelectedMaterial = null;
        LicensePlate = string.Empty;
        Transporter = string.Empty;
        Weight = string.Empty;
        Suppliers.Clear();
        Materials.Clear();
    }

    // Cargar los catalogos activos
    public async Task LoadCatalogsAsync()
    {
        if (IsBusy || IsRegistered) return;

        IsLoadingCatalogs = true;
        ErrorMessage = string.Empty;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var getSuppliers = scope.ServiceProvider.GetRequiredService<GetActiveSuppliers>();
            var getMaterials = scope.ServiceProvider.GetRequiredService<GetActiveMaterials>();

            var suppliers = await getSuppliers.ExecuteAsync();
            var materials = await getMaterials.ExecuteAsync();

            Suppliers.Clear();
            Materials.Clear();
            foreach (var supplier in suppliers) Suppliers.Add(supplier);
            foreach (var material in materials) Materials.Add(material);
        }
        catch (Exception ex)
        {
            ErrorMessage = "No fue posible cargar los proveedores y materiales.";
            Console.Error.WriteLine(ex);
        }
        finally
        {
            IsLoadingCatalogs = false;
        }
    }

    private bool CanReadWeight() => ShowForm && !IsBusy;

    // Leer el indicador, sin eliminar el peso manual si falla
    private async Task ReadWeightAsync()
    {
        if (!CanReadWeight()) return;
        IsReadingWeight = true;
        ErrorMessage = string.Empty;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var readWeight = scope.ServiceProvider.GetRequiredService<ReadWeightUseCase>();
            decimal weight = await readWeight.ExecuteAsync();

            if (weight <= 0)
            {
                ErrorMessage = "El indicador devolvió un peso inválido.";
                return;
            }

            Weight = weight.ToString("0.############################", CultureInfo.CurrentCulture);
        }
        catch (TimeoutException ex)
        {
            ErrorMessage = "El indicador no respondió a tiempo. Puedes introducir el peso manualmente.";
            Console.Error.WriteLine(ex);
        }
        catch (IOException ex)
        {
            ErrorMessage = "Ocurrió un error de comunicación con el indicador. Puedes introducir el peso manualmente.";
            Console.Error.WriteLine(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ErrorMessage = "No fue posible acceder al puerto del indicador. Puedes introducir el peso manualmente.";
            Console.Error.WriteLine(ex);
        }
        catch (Exception ex)
        {
            ErrorMessage = "No fue posible obtener el peso del indicador. Puedes introducirlo manualmente.";
            Console.Error.WriteLine(ex);
        }
        finally
        {
            IsReadingWeight = false;
        }
    }

    private bool CanSave() => !IsRegistered && !IsBusy;

    // Registrar un ticket nuevo una sola vez
    private async Task SaveAsync()
    {
        if (!CanSave()) return;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (!TryValidateForm(out decimal weight)) return;

        IsSaving = true;
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            int ticketId;
            string ticketNumber;

            if (IsConventional)
            {
                var createTicket = scope.ServiceProvider.GetRequiredService<CreateConventionalPurchaseTicket>();
                var command = new CreateConventionalPurchaseTicketCommand(
                    SelectedSupplier!.Id, SelectedMaterial!.Id, LicensePlate, Transporter, weight);
                var result = await createTicket.ExecuteAsync(command);
                ticketId = result.Id;
                ticketNumber = result.TicketNumber;
            }
            else
            {
                var createTicket = scope.ServiceProvider.GetRequiredService<CreateSinglePurchaseTicket>();
                var command = new CreateSinglePurchaseTicketCommand(
                    SelectedSupplier!.Id,
                    SelectedMaterial!.Id,
                    string.IsNullOrWhiteSpace(LicensePlate) ? null : LicensePlate,
                    Transporter,
                    weight);
                var result = await createTicket.ExecuteAsync(command);
                ticketId = result.Id;
                ticketNumber = result.TicketNumber;
            }

            TicketId = ticketId;
            TicketNumber = ticketNumber;
            _savedSnapshot = CaptureSnapshot(weight);
            IsRegistered = true;
            SuccessMessage = $"El ticket {ticketNumber} se registró correctamente.";
        }
        catch (Exception ex)
            when (ex is BusinessRuleException or NotFoundException)
        {
            ErrorMessage = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "No fue posible registrar el pesaje.";
            Console.Error.WriteLine(ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    // Abrir el formulario de correccion sin crear otro ticket
    private void BeginEdit()
    {
        if (!IsRegistered || IsEditing || IsBusy || !TicketId.HasValue || _savedSnapshot is null)
            return;

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        IsEditing = true;
    }

    // Cancelar los cambios realizados en pantalla
    private void CancelEdit()
    {
        if (!IsRegistered || !IsEditing || IsBusy || _savedSnapshot is null)
            return;

        RestoreSnapshot(_savedSnapshot);
        IsEditing = false;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    private bool CanUpdate() => IsRegistered && IsEditing && TicketId.HasValue && !IsBusy;

    // Corregir solamente los campos modificados
    private async Task UpdateAsync()
    {
        if (!CanUpdate() || _savedSnapshot is null) return;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (!TryValidateForm(out decimal weight)) return;

        var before = _savedSnapshot;
        var current = CaptureSnapshot(weight);

        int? supplierId = current.SupplierId != before.SupplierId ? current.SupplierId : null;
        int? materialId = current.MaterialId != before.MaterialId ? current.MaterialId : null;
        string? transporter = !string.Equals(current.Transporter.Trim(), before.Transporter.Trim(), StringComparison.Ordinal)
            ? current.Transporter
            : null;
        bool changeLicensePlate = !string.Equals(
            NormalizePlate(current.LicensePlate), NormalizePlate(before.LicensePlate), StringComparison.Ordinal);
        bool changeWeight = current.Weight != before.Weight;

        if (supplierId is null && materialId is null && transporter is null && !changeLicensePlate && !changeWeight)
        {
            IsEditing = false;
            SuccessMessage = "No se realizaron cambios en el ticket.";
            return;
        }

        IsSaving = true;
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var correctTicket = scope.ServiceProvider.GetRequiredService<CorrectPurchaseTicket>();

            var command = new CorrectPurchaseTicketCommand(
                TicketId: TicketId!.Value,
                SupplierId: supplierId,
                MaterialId: materialId,
                Transporter: transporter,
                ChangeLicensePlate: changeLicensePlate,
                LicensePlate: changeLicensePlate ? current.LicensePlate : null,
                GrossWeight: changeWeight && IsConventional ? weight : null,
                NetWeight: changeWeight && IsSingle ? weight : null);

            await correctTicket.ExecuteAsync(command);

            _savedSnapshot = current;
            IsEditing = false;
            SuccessMessage = $"Los cambios del ticket {TicketNumber} se guardaron correctamente.";
        }
        catch (Exception ex)
            when (ex is BusinessRuleException or NotFoundException)
        {
            ErrorMessage = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "No fue posible corregir el ticket. Los cambios no se confirmaron.";
            Console.Error.WriteLine(ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanPrintTicket() => ShowSummary && TicketId.HasValue && !IsBusy;

    // Enviar el ticket registrado a impresion
    private async Task PrintTicketAsync()
    {
        if (!CanPrintTicket()) return;

        IsPrinting = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            // Ejecutar la impresion fuera del hilo de WPF
            var ticketId = TicketId!.Value;

            var result = await Task.Run(async () =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                var printTicket = scope.ServiceProvider
                    .GetRequiredService<PrintPurchaseTicket>();

                var command = new PrintPurchaseTicketCommand(ticketId);

                return await printTicket.ExecuteAsync(command);
            });

            if (result.Printed)
            {
                SuccessMessage =
                    $"El ticket {result.TicketNumber} se envió correctamente a impresión.";
            }
            else
            {
                ErrorMessage = result.ErrorCode switch
                {
                    PrinterErrorCode.PortNotFound =>
                        "El puerto configurado para la impresora no está disponible. " +
                        "Verifica la configuración de conexión.",

                    PrinterErrorCode.PortAccessDenied =>
                        "No fue posible acceder al puerto de la impresora. " +
                        "Verifica que no esté siendo utilizado por otra aplicación.",

                    PrinterErrorCode.CommunicationError =>
                        "Ocurrió un error de comunicación con la impresora. " +
                        "Verifica la conexión e inténtalo nuevamente.",

                    _ =>
                        "No fue posible imprimir el ticket. " +
                        "Puedes intentarlo nuevamente."
                };
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al solicitar la impresión. El ticket permanece registrado.";
            Console.Error.WriteLine(ex);
        }
        finally
        {
            IsPrinting = false;
        }
    }

    // Validaciones antes de registrar o corregir
    private bool TryValidateForm(out decimal weight)
    {
        weight = 0;
        if (SelectedSupplier is null)
        {
            ErrorMessage = "Selecciona un proveedor.";
            return false;
        }
        if (SelectedMaterial is null)
        {
            ErrorMessage = "Selecciona un material.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(Transporter))
        {
            ErrorMessage = "El transportista es obligatorio.";
            return false;
        }
        if (IsConventional && string.IsNullOrWhiteSpace(LicensePlate))
        {
            ErrorMessage = "Las placas son obligatorias para el pesaje convencional.";
            return false;
        }
        if (!decimal.TryParse(Weight, NumberStyles.Number, CultureInfo.CurrentCulture, out weight) || weight <= 0)
        {
            ErrorMessage = "Introduce un peso válido mayor a cero.";
            return false;
        }
        return true;
    }

    private TicketSnapshot CaptureSnapshot(decimal weight) => new(
        SelectedSupplier!.Id,
        SelectedMaterial!.Id,
        string.IsNullOrWhiteSpace(LicensePlate) ? null : LicensePlate,
        Transporter,
        weight);

    private void RestoreSnapshot(TicketSnapshot snapshot)
    {
        SelectedSupplier = Suppliers.FirstOrDefault(x => x.Id == snapshot.SupplierId);
        SelectedMaterial = Materials.FirstOrDefault(x => x.Id == snapshot.MaterialId);
        LicensePlate = snapshot.LicensePlate ?? string.Empty;
        Transporter = snapshot.Transporter;
        Weight = snapshot.Weight.ToString("0.############################", CultureInfo.CurrentCulture);
    }

    private static string NormalizePlate(string? value) =>
        (value ?? string.Empty).Trim().ToUpperInvariant();
}
