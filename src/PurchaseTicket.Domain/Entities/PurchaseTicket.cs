using System.Globalization;
using System.Text.RegularExpressions;
using PurchaseTicket.Domain.Enums;
using PurchaseTicket.Domain.Exceptions;
using PurchaseTicket.Domain.ValueObjects;

namespace PurchaseTicket.Domain.Entities;

public class PurchaseTicket
{
    // Campos

    // Palabras de letras, unidas opcionalmente por guion o apostrofo,
    // separadas por un solo espacio. Ej.: "Juan Perez-Lopez", "O'Neil".
    private static readonly Regex TransporterPattern = new(
        @"^\p{L}+(?:['-]\p{L}+)*(?: \p{L}+(?:['-]\p{L}+)*)*$",
        RegexOptions.Compiled);

    // Constructor

    private PurchaseTicket()
    {
    }

    // Propiedades

    public int Id { get; private set; }

    public string TicketNumber { get; private set; } = string.Empty;

    public int SupplierId { get; private set; }

    public int MaterialId { get; private set; }

    public string? LicensePlate { get; private set; }

    public string Transporter { get; private set; } = string.Empty;

    public DateTime CheckInAt { get; private set; }

    public DateTime? DepartureAt { get; private set; }

    public decimal? GrossWeight { get; private set; }

    public decimal? TareWeight { get; private set; }

    public decimal? NetWeight { get; private set; }

    public int? Discount { get; private set; }

    public decimal? DiscountWeight { get; private set; }

    public decimal? NetWeightAfterDiscount { get; private set; }

    public decimal? PricePerKg { get; private set; }

    public decimal? Amount { get; private set; }

    public TicketStatus Status { get; private set; }

    public WeighingType WeighingType { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    // Metodos publicos

    // Methods
    public static PurchaseTicket CreateConventional(
        string ticketNumber,
        int supplierId,
        int materialId,
        string licensePlate,
        string transporter,
        decimal grossWeight)
    {
        ValidateTicketNumber(ticketNumber);
        ValidateSupplierId(supplierId);
        ValidateMaterialId(materialId);
        ValidateGrossWeight(grossWeight);

        string normalizedLicensePlate =
            NormalizeLicensePlate(licensePlate);

        string normalizedTransporter =
            NormalizeTransporter(transporter);

        var now = DateTime.UtcNow;

        return new PurchaseTicket
        {
            TicketNumber = ticketNumber.Trim(),
            SupplierId = supplierId,
            MaterialId = materialId,
            LicensePlate = normalizedLicensePlate,
            Transporter = normalizedTransporter,
            GrossWeight = grossWeight,
            CheckInAt = now,
            CreatedAt = now,
            WeighingType = WeighingType.Conventional,
            Status = TicketStatus.WeighingPending
        };
    }

    public static PurchaseTicket CreateSingle(
        string ticketNumber,
        int supplierId,
        int materialId,
        string? licensePlate,
        string transporter,
        decimal netWeight)
    {
        ValidateTicketNumber(ticketNumber);
        ValidateSupplierId(supplierId);
        ValidateMaterialId(materialId);
        ValidateNetWeight(netWeight);

        string? normalizedLicensePlate =
            NormalizeOptionalLicensePlate(licensePlate);

        string normalizedTransporter =
            NormalizeTransporter(transporter);

        var now = DateTime.UtcNow;

        return new PurchaseTicket
        {
            TicketNumber = ticketNumber.Trim(),
            SupplierId = supplierId,
            MaterialId = materialId,
            LicensePlate = normalizedLicensePlate,
            Transporter = normalizedTransporter,
            GrossWeight = null,
            TareWeight = null,
            NetWeight = netWeight,
            CheckInAt = now,
            DepartureAt = null,
            CreatedAt = now,
            WeighingType = WeighingType.Single,
            Status = TicketStatus.AmountPending
        };
    }

    // Publicos para que la interfaz valide cada campo con las mismas reglas,
    // igual que Supplier.NormalizeName
    public static string NormalizeTransporter(string transporter)
    {
        if (string.IsNullOrWhiteSpace(transporter))
            throw new ArgumentException("El transportista es obligatorio.");

        transporter = string.Join(
            " ",
            transporter
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));

        if (!TransporterPattern.IsMatch(transporter))
            throw new ArgumentException(
                "El transportista solo puede contener letras, espacios, guiones y apóstrofos.");

        TextInfo textInfo =
            CultureInfo
                .GetCultureInfo("es-MX")
                .TextInfo;

        transporter = textInfo.ToTitleCase(
            transporter.ToLower(
                CultureInfo.GetCultureInfo("es-MX")));

        if (transporter.Length > 50)
            throw new ArgumentException("El transportista no puede exceder los 50 caracteres.");

        return transporter;
    }

    public static string? NormalizeOptionalLicensePlate(string? licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            return null;

        return NormalizeLicensePlateValue(
            licensePlate);
    }

    public static string NormalizeLicensePlate(string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            throw new ArgumentException("Las placas son obligatorias.");

        return NormalizeLicensePlateValue(
            licensePlate);
    }

    public static void ValidateDiscount(int discount)
    {
        if (discount < 0 || discount > 99)
            throw new ArgumentException(
                "El descuento debe ser un número entero entre 0 y 99.",
                nameof(discount));
    }

    public static void ValidatePricePerKg(decimal pricePerKg)
    {
        if (pricePerKg <= 0)
            throw new ArgumentException("El precio por kilogramo debe ser mayor a cero.");

        // La base de datos guarda dos decimales: con mas, el importe calculado
        // no coincidiria con el precio guardado
        if (decimal.Round(pricePerKg, 2) != pricePerKg)
            throw new ArgumentException("El precio por kilogramo no puede tener más de dos decimales.");
    }

    // Publico para que la ventana de importe muestre exactamente lo que se
    // guardara, sin repetir la formula. Se calcula con precision completa y
    // solo se redondea el resultado (RN-41): con pesos enteros coincide con
    // redondear cada paso; con decimales el importe puede diferir un centavo
    // de multiplicar las cifras redondeadas, a cambio de ser el valor exacto
    public static AmountBreakdown CalculateAmountBreakdown(
        decimal netWeight,
        int discount,
        decimal pricePerKg)
    {
        decimal discountWeight =
            CalculateDiscountWeight(
                netWeight,
                discount);

        decimal netWeightAfterDiscount =
            CalculateNetWeightAfterDiscount(
                netWeight,
                discountWeight);

        decimal amount =
            CalculateAmount(
                netWeightAfterDiscount,
                pricePerKg);

        return new AmountBreakdown(
            RoundToTwoDecimals(discountWeight),
            RoundToTwoDecimals(netWeightAfterDiscount),
            RoundToTwoDecimals(amount));
    }

    public void Cancel()
    {
        if (Status == TicketStatus.Cancelled)
            throw new BusinessRuleException(
                "El ticket ya se encuentra cancelado.");

        if (Status == TicketStatus.Completed)
            throw new BusinessRuleException(
                "Un ticket completado no puede cancelarse.");

        Status = TicketStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegisterTare(decimal tareWeight)
    {
        ValidateConventionalWeighing();
        ValidateWeighingPendingStatus();

        decimal grossWeight = GrossWeight
            ?? throw new InvalidOperationException(
                "El pesaje convencional debe tener un peso bruto.");

        ValidateTareWeight(tareWeight, grossWeight);

        TareWeight = tareWeight;

        NetWeight = CalculateNetWeight(grossWeight, tareWeight);

        DepartureAt = DateTime.UtcNow;

        Status = TicketStatus.AmountPending;
    }

    public void RegisterAmount(int discount, decimal pricePerKg)
    {
        ValidateAmountPendingStatus();

        if (NetWeight is null)
            throw new InvalidOperationException("El ticket debe tener un peso neto.");

        ValidateDiscount(discount);
        ValidatePricePerKg(pricePerKg);

        Discount = discount;
        PricePerKg = pricePerKg;

        RecalculateAmount();

        Status = TicketStatus.Completed;
    }

    public void ChangeSupplier(int supplierId)
    {
        ValidateCanModifyGeneralData();
        ValidateSupplierId(supplierId);

        SupplierId = supplierId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeMaterial(int materialId)
    {
        ValidateCanModifyGeneralData();
        ValidateMaterialId(materialId);

        MaterialId = materialId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeLicensePlate(string? licensePlate)
    {
        ValidateCanModifyGeneralData();

        LicensePlate = WeighingType == WeighingType.Conventional
            ? NormalizeLicensePlate(licensePlate!)
            : NormalizeOptionalLicensePlate(licensePlate);

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeTransporter(string transporter)
    {
        ValidateCanModifyGeneralData();

        Transporter = NormalizeTransporter(transporter);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeGrossWeight(decimal grossWeight)
    {
        ValidateConventionalWeighing();
        ValidateCanModifyGrossWeight();
        ValidateGrossWeight(grossWeight);

        if (Status == TicketStatus.WeighingPending)
        {
            GrossWeight = grossWeight;
            UpdatedAt = DateTime.UtcNow;
            return;
        }

        decimal tareWeight = TareWeight
            ?? throw new InvalidOperationException(
                "El pesaje convencional debe tener una tara.");

        ValidateTareWeight(tareWeight, grossWeight);

        GrossWeight = grossWeight;

        NetWeight = CalculateNetWeight(
            grossWeight,
            tareWeight);

        RecalculateAmount();

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeTareWeight(decimal tareWeight)
    {
        ValidateConventionalWeighing();
        ValidateCanModifyTareWeight();

        decimal grossWeight = GrossWeight
            ?? throw new InvalidOperationException(
                "El pesaje convencional debe tener un peso bruto.");

        ValidateTareWeight(
            tareWeight,
            grossWeight);

        TareWeight = tareWeight;

        NetWeight = CalculateNetWeight(
            grossWeight,
            tareWeight);

        if (Status == TicketStatus.Completed)
        {
            RecalculateAmount();
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeDiscount(int discount)
    {
        ValidateCompletedStatus();
        ValidateDiscount(discount);

        Discount = discount;

        RecalculateAmount();

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePricePerKg(decimal pricePerKg)
    {
        ValidateCompletedStatus();
        ValidatePricePerKg(pricePerKg);

        PricePerKg = pricePerKg;

        RecalculateAmount();

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeNetWeight(decimal netWeight)
    {
        ValidateSingleWeighing();
        ValidateCanModifyNetWeight();
        ValidateNetWeight(netWeight);

        NetWeight = netWeight;

        if (Status == TicketStatus.Completed)
        {
            RecalculateAmount();
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Correct(PurchaseTicketCorrection correction)
    {
        bool hasGeneralDataChanges =
            correction.SupplierId.HasValue ||
            correction.MaterialId.HasValue ||
            correction.Transporter is not null ||
            correction.ChangeLicensePlate;

        bool hasGrossWeightChange = correction.GrossWeight.HasValue;

        bool hasTareWeightChange = correction.TareWeight.HasValue;

        bool hasConventionalWeightChanges = hasGrossWeightChange || hasTareWeightChange;

        bool hasNetWeightChange = correction.NetWeight.HasValue;

        bool hasDiscountChange = correction.Discount.HasValue;

        bool hasPricePerKgChange = correction.PricePerKg.HasValue;

        if (!hasGeneralDataChanges &&
            !hasConventionalWeightChanges &&
            !hasNetWeightChange &&
            !hasDiscountChange &&
            !hasPricePerKgChange)
        {
            return;
        }

        // 1. Validate general data changes

        if (hasGeneralDataChanges)
        {
            ValidateCanModifyGeneralData();
        }

        if (correction.SupplierId.HasValue)
            ValidateSupplierId(correction.SupplierId.Value);

        if (correction.MaterialId.HasValue)
            ValidateMaterialId(correction.MaterialId.Value);

        string? normalizedTransporter = null;

        if (correction.Transporter is not null)
            normalizedTransporter = NormalizeTransporter(correction.Transporter);

        string? normalizedLicensePlate = null;

        if (correction.ChangeLicensePlate)
        {
            normalizedLicensePlate =
                WeighingType == WeighingType.Conventional
                    ? NormalizeLicensePlate(
                        correction.LicensePlate!)
                    : NormalizeOptionalLicensePlate(
                        correction.LicensePlate);
        }

        // 2. Validate conventional weight changes

        if (hasConventionalWeightChanges)
        {
            ValidateConventionalWeighing();

            if (hasGrossWeightChange)
            {
                ValidateCanModifyGrossWeight();

                ValidateGrossWeight(correction.GrossWeight!.Value);
            }

            if (hasTareWeightChange)
                ValidateCanModifyTareWeight();
        }

        // 3. Validate single net weight change

        if (hasNetWeightChange)
        {
            ValidateSingleWeighing();
            ValidateCanModifyNetWeight();

            ValidateNetWeight(correction.NetWeight!.Value);
        }

        // 4. Validate discount change

        if (hasDiscountChange)
        {
            ValidateCompletedStatus();

            ValidateDiscount(correction.Discount!.Value);
        }

        // 5. Validate price per kg change

        if (hasPricePerKgChange)
        {
            ValidateCompletedStatus();

            ValidatePricePerKg(correction.PricePerKg!.Value);
        }

        // 6. Calculate resulting conventional weights
        // without modifying the entity

        decimal? resultingGrossWeight =
            correction.GrossWeight ??
            GrossWeight;

        decimal? resultingTareWeight =
            correction.TareWeight ??
            TareWeight;

        if (hasTareWeightChange || (hasGrossWeightChange && Status == TicketStatus.Completed))
        {
            decimal grossWeight =
                resultingGrossWeight
                ?? throw new InvalidOperationException("El pesaje convencional debe tener un peso bruto.");

            decimal tareWeight =
                resultingTareWeight
                ?? throw new InvalidOperationException("El pesaje convencional debe tener una tara.");

            ValidateTareWeight(tareWeight, grossWeight);
        }

        // 7. Apply conventional weight changes

        if (hasGrossWeightChange)
            GrossWeight = correction.GrossWeight!.Value;

        if (hasTareWeightChange)
            TareWeight = correction.TareWeight!.Value;

        // 8. Recalculate conventional net weight

        if (hasConventionalWeightChanges &&
            (Status == TicketStatus.AmountPending ||
             Status == TicketStatus.Completed))
        {
            decimal grossWeight =
                resultingGrossWeight
                ?? throw new InvalidOperationException("El pesaje convencional debe tener un peso bruto.");

            decimal tareWeight =
                resultingTareWeight
                ?? throw new InvalidOperationException("El pesaje convencional debe tener una tara.");

            NetWeight = CalculateNetWeight(grossWeight, tareWeight);
        }

        // 9. Apply single net weight change

        if (hasNetWeightChange)
            NetWeight = correction.NetWeight!.Value;

        // 10. Apply discount change

        if (hasDiscountChange)
            Discount = correction.Discount!.Value;

        // 11. Apply price per kg change

        if (hasPricePerKgChange)
            PricePerKg = correction.PricePerKg!.Value;

        // 12. Recalculate amount once

        bool mustRecalculateAmount = Status == TicketStatus.Completed &&
            (
                hasConventionalWeightChanges ||
                hasNetWeightChange ||
                hasDiscountChange ||
                hasPricePerKgChange);

        if (mustRecalculateAmount)
            RecalculateAmount();

        // 13. Apply general data changes

        if (correction.SupplierId.HasValue)
            SupplierId = correction.SupplierId.Value;

        if (correction.MaterialId.HasValue)
            MaterialId = correction.MaterialId.Value;

        if (normalizedTransporter is not null)
            Transporter = normalizedTransporter;

        if (correction.ChangeLicensePlate)
            LicensePlate = normalizedLicensePlate;

        UpdatedAt = DateTime.UtcNow;
    }

    // Metodos privados

    private static string NormalizeLicensePlateValue(string licensePlate)
    {
        licensePlate =
            licensePlate.Trim().ToUpperInvariant();

        if (licensePlate.Length > 7)
            throw new ArgumentException("Las placas no pueden exceder los 7 caracteres.");

        if (!licensePlate.All(char.IsLetterOrDigit))
            throw new ArgumentException("Las placas solo pueden contener caracteres alfanuméricos.");

        return licensePlate;
    }

    private static void ValidateTareWeight(decimal tareWeight, decimal grossWeight)
    {
        if (tareWeight <= 0)
            throw new ArgumentException("La tara debe ser mayor a cero.");

        if (tareWeight >= grossWeight)
            throw new ArgumentException("La tara debe ser menor al peso bruto.");
    }

    private static void ValidateTicketNumber(string ticketNumber)
    {
        if (string.IsNullOrWhiteSpace(ticketNumber))
            throw new ArgumentException("El folio es obligatorio.");

        if (ticketNumber.Trim().Length > 20)
            throw new ArgumentException("El folio no puede exceder los 20 caracteres.");
    }

    private static void ValidateSupplierId(int supplierId)
    {
        if (supplierId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(supplierId),
                "El identificador del proveedor debe ser mayor a cero.");
    }

    private static void ValidateMaterialId(int materialId)
    {
        if (materialId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(materialId),
                "El identificador del material debe ser mayor a cero.");
    }

    private static void ValidateGrossWeight(decimal grossWeight)
    {
        if (grossWeight <= 0)
            throw new ArgumentException("El peso bruto debe ser mayor a cero.");
    }

    private static void ValidateNetWeight(decimal netWeight)
    {
        if (netWeight <= 0)
            throw new ArgumentException("El peso neto debe ser mayor a cero.");
    }

    private static decimal CalculateNetWeight(decimal grossWeight, decimal tareWeight)
    {
        return grossWeight - tareWeight;
    }

    private static decimal CalculateDiscountWeight(decimal netWeight, int discount)
    {
        return netWeight * discount / 100;
    }

    private static decimal CalculateNetWeightAfterDiscount(decimal netWeight, decimal discountWeight)
    {
        return netWeight - discountWeight;
    }

    private static decimal CalculateAmount(decimal netWeightAfterDiscount, decimal pricePerKg)
    {
        return netWeightAfterDiscount * pricePerKg;
    }

    // Igual que REDONDEAR de Excel y que PostgreSQL: el 5 se aleja de cero
    // (2.345 -> 2.35). El redondeo por defecto de .NET daria 2.34
    private static decimal RoundToTwoDecimals(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private void RecalculateAmount()
    {
        decimal netWeight = NetWeight
            ?? throw new InvalidOperationException(
                "El ticket debe tener un peso neto.");

        int discount = Discount
            ?? throw new InvalidOperationException(
                "El ticket debe tener un descuento.");

        decimal pricePerKg = PricePerKg
            ?? throw new InvalidOperationException(
                "El ticket debe tener un precio por kilogramo.");

        var breakdown = CalculateAmountBreakdown(
            netWeight,
            discount,
            pricePerKg);

        DiscountWeight = breakdown.DiscountWeight;
        NetWeightAfterDiscount = breakdown.NetWeightAfterDiscount;
        Amount = breakdown.Amount;
    }

    private void ValidateConventionalWeighing()
    {
        if (WeighingType != WeighingType.Conventional)
            throw new BusinessRuleException("Esta operación solo aplica a un pesaje convencional.");
    }

    private void ValidateWeighingPendingStatus()
    {
        if (Status != TicketStatus.WeighingPending)
        {
            throw new BusinessRuleException(
                "El ticket debe estar pendiente de pesaje.");
        }
    }

    private void ValidateAmountPendingStatus()
    {
        if (Status != TicketStatus.AmountPending)
            throw new BusinessRuleException("El ticket debe estar pendiente de importe.");
    }

    private void ValidateCanModifyGeneralData()
    {
        // El pesaje unico nace pendiente de importe, asi que ese es el
        // momento en que pesaje puede corregir sus datos generales
        bool canModify =
            Status == TicketStatus.WeighingPending ||
            Status == TicketStatus.Completed ||
            (Status == TicketStatus.AmountPending &&
             WeighingType == WeighingType.Single);

        if (!canModify)
            throw new BusinessRuleException(
                "El ticket no permite modificar este campo en su estado actual.");
    }

    private void ValidateCanModifyTareWeight()
    {
        if (Status != TicketStatus.AmountPending && Status != TicketStatus.Completed)
            throw new BusinessRuleException("La tara no puede modificarse en el estado actual del ticket.");
    }

    private void ValidateCanModifyGrossWeight()
    {
        if (Status != TicketStatus.WeighingPending && Status != TicketStatus.Completed)
            throw new BusinessRuleException("El peso bruto no puede modificarse en el estado actual del ticket.");
    }

    private void ValidateCompletedStatus()
    {
        if (Status != TicketStatus.Completed)
            throw new BusinessRuleException("El ticket debe estar completado.");
    }

    private void ValidateSingleWeighing()
    {
        if (WeighingType != WeighingType.Single)
            throw new BusinessRuleException(
                "El peso neto solo puede modificarse directamente en un pesaje único.");
    }

    private void ValidateCanModifyNetWeight()
    {
        if (Status != TicketStatus.AmountPending && Status != TicketStatus.Completed)
            throw new BusinessRuleException(
                "El peso neto no puede modificarse en el estado actual del ticket.");
    }
}