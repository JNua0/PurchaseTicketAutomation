namespace PurchaseTicket.Domain.Entities;

public class Material
{
    // Constructor

    public Material(string name)
    {
        Name = NormalizeName(name);
        IsActive = true;
    }

    // Propiedades

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    // Metodos publicos

    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "El nombre del material es obligatorio.");

        name = name.Trim();

        if (name.Length > 50)
            throw new ArgumentException(
                "El nombre del material no puede exceder los 50 caracteres.");

        name = name.ToLower();

        return char.ToUpper(name[0]) + name[1..];
    }

    public void UpdateName(string name)
    {
        Name = NormalizeName(name);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}