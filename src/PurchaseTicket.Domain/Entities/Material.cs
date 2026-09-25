namespace PurchaseTicket.Domain.Entities;

public class Material
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public Material(string name)
    {
        Name = NormalizeName(name);
        IsActive = true;
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

    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Material name is required."
            );

        name = name.Trim();

        if (name.Length > 50)
            throw new ArgumentException(
                "Material name cannot exceed 50 characters."
            );

        name = name.ToLower();

        return char.ToUpper(name[0]) + name[1..];
    }
}