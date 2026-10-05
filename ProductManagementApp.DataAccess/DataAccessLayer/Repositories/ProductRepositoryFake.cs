using ProductManagementApp.DataAccessLayer.Interfaces;
using ProductManagementApp.Models;

namespace ProductManagementApp.DataAccessLayer.Repositories;

/// <summary>
/// In-memory repository used to prove that UI and business layers depend on
/// IProductRepository, not on JSON. Data is lost when the process stops.
/// </summary>
public class ProductRepositoryFake : IProductRepository
{
    private readonly List<ProductModel> _products =
    [
        new()
        {
            Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeee1"),
            Name = "Fake Laptop",
            Price = 99999,
            Category = "Electronics",
            Description = "In-memory sample product",
            IsActive = true
        },
        new()
        {
            Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeee2"),
            Name = "Fake Mouse",
            Price = 1500,
            Category = "Accessories",
            Description = "In-memory sample product",
            IsActive = true
        }
    ];

    public Task<List<ProductModel>> GetAllAsync()
    {
        return Task.FromResult(_products.Select(Clone).ToList());
    }

    public Task<ProductModel?> GetByIdAsync(Guid id)
    {
        var match = _products.FirstOrDefault(product => product.Id == id);
        return Task.FromResult(match is null ? null : Clone(match));
    }

    public Task AddAsync(ProductModel product)
    {
        _products.Add(Clone(product));
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ProductModel product)
    {
        var existing = _products.FirstOrDefault(item => item.Id == product.Id)
            ?? throw new KeyNotFoundException($"Product '{product.Id}' was not found.");

        existing.Name = product.Name;
        existing.Price = product.Price;
        existing.Category = product.Category;
        existing.Description = product.Description;
        existing.IsActive = product.IsActive;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var existing = _products.FirstOrDefault(item => item.Id == id)
            ?? throw new KeyNotFoundException($"Product '{id}' was not found.");

        _products.Remove(existing);
        return Task.CompletedTask;
    }

    public Task<List<ProductModel>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return GetAllAsync();
        }

        var results = _products
            .Where(product =>
                Contains(product.Name, keyword)
                || Contains(product.Category, keyword)
                || Contains(product.Description, keyword))
            .Select(Clone)
            .ToList();

        return Task.FromResult(results);
    }

    private static ProductModel Clone(ProductModel product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        Category = product.Category,
        Description = product.Description,
        IsActive = product.IsActive
    };

    private static bool Contains(string? value, string keyword) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Contains(keyword, StringComparison.OrdinalIgnoreCase);
}
