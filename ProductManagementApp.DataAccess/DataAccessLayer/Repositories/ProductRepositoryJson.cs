using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using ProductManagementApp.DataAccessLayer.Exceptions;
using ProductManagementApp.DataAccessLayer.Interfaces;
using ProductManagementApp.Models;

namespace ProductManagementApp.DataAccessLayer.Repositories;

public class ProductRepositoryJson : IProductRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public ProductRepositoryJson(IWebHostEnvironment environment)
    {
        var contentRootPath = Path.Combine(environment.ContentRootPath, "Data", "products.json");
        var outputPath = Path.Combine(AppContext.BaseDirectory, "Data", "products.json");

        // Prefer the project Data folder in development so edits persist in source.
        // Fall back to the copied output file when the content-root file is absent.
        _filePath = File.Exists(contentRootPath) || !File.Exists(outputPath)
            ? contentRootPath
            : outputPath;
    }

    public async Task<List<ProductModel>> GetAllAsync()
    {
        return await ReadProductsAsync();
    }

    public async Task<ProductModel?> GetByIdAsync(Guid id)
    {
        var products = await ReadProductsAsync();
        return products.FirstOrDefault(product => product.Id == id);
    }

    public async Task AddAsync(ProductModel product)
    {
        await _gate.WaitAsync();
        try
        {
            var products = await ReadProductsUnlockedAsync();
            products.Add(product);
            await WriteProductsUnlockedAsync(products);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task UpdateAsync(ProductModel product)
    {
        await _gate.WaitAsync();
        try
        {
            var products = await ReadProductsUnlockedAsync();
            var existing = products.FirstOrDefault(item => item.Id == product.Id);
            if (existing is null)
            {
                throw new KeyNotFoundException($"Product '{product.Id}' was not found.");
            }

            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Category = product.Category;
            existing.Description = product.Description;
            existing.IsActive = product.IsActive;

            await WriteProductsUnlockedAsync(products);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            var products = await ReadProductsUnlockedAsync();
            var existing = products.FirstOrDefault(item => item.Id == id);
            if (existing is null)
            {
                throw new KeyNotFoundException($"Product '{id}' was not found.");
            }

            products.Remove(existing);
            await WriteProductsUnlockedAsync(products);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<List<ProductModel>> SearchAsync(string keyword)
    {
        var products = await ReadProductsAsync();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return products;
        }

        return products
            .Where(product => Matches(product, keyword))
            .ToList();
    }

    private async Task<List<ProductModel>> ReadProductsAsync()
    {
        await _gate.WaitAsync();
        try
        {
            return await ReadProductsUnlockedAsync();
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<ProductModel>> ReadProductsUnlockedAsync()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<ProductModel>>(json, JsonOptions) ?? [];
        }
        catch (JsonException exception)
        {
            throw new ProductDataException(
                "Product data could not be read because products.json is invalid.",
                exception);
        }
    }

    private async Task WriteProductsUnlockedAsync(List<ProductModel> products)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(products, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }

    private static bool Matches(ProductModel product, string keyword)
    {
        return Contains(product.Name, keyword)
            || Contains(product.Category, keyword)
            || Contains(product.Description, keyword);
    }

    private static bool Contains(string? value, string keyword)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }
}
