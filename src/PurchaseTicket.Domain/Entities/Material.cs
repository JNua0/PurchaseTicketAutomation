namespace PurchaseTicket.Domain.Entities;

public class Material
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public Material(string name)
    {
        Name = NormalizeName(name);
    }

    private static string NormalizeName(string name)
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

        return name;
    }
}