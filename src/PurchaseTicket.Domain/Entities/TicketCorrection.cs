using PurchaseTicket.Domain.ValueObjects;

namespace PurchaseTicket.Domain.Entities;

// Una correccion guardada de un ticket: cuando fue, por que y que cambio.
// Solo se crea desde PurchaseTicket.RecordCorrection y no se modifica
public class TicketCorrection
{
    // Campos

    public const int ReasonMaxLength = 200;

    private readonly List<TicketFieldChange> _changes = [];

    // Constructor

    private TicketCorrection()
    {
    }

    internal TicketCorrection(
        string? reason,
        IEnumerable<TicketFieldChange> changes)
    {
        _changes.AddRange(changes);

        if (_changes.Count == 0)
            throw new ArgumentException("Una corrección debe tener al menos un cambio.");

        Reason = NormalizeReason(reason);
        CorrectedAt = DateTime.UtcNow;
    }

    // Propiedades

    public int Id { get; private set; }

    public int PurchaseTicketId { get; private set; }

    public DateTime CorrectedAt { get; private set; }

    public string? Reason { get; private set; }

    public IReadOnlyList<TicketFieldChange> Changes => _changes;

    // Metodos publicos

    // Opcional: vacio o solo espacios se guarda como null
    public static string? NormalizeReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return null;

        reason = reason.Trim();

        if (reason.Length > ReasonMaxLength)
            throw new ArgumentException(
                $"El motivo no puede exceder los {ReasonMaxLength} caracteres.");

        return reason;
    }
}
