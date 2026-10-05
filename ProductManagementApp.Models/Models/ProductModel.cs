namespace ProductManagementApp.Models;

/// <summary>
/// Domain model for a retail product.
/// Name and Category are non-nullable because they are required by business rules.
/// Description is nullable because a product can be stored without extra notes.
/// Id, Price, and IsActive are value types and cannot be null.
/// </summary>
public class ProductModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Category { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
