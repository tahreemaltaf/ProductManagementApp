using ProductManagementApp.Business.Exceptions;
using ProductManagementApp.Business.Interfaces;
using ProductManagementApp.DataAccessLayer.Interfaces;
using ProductManagementApp.Models;

namespace ProductManagementApp.Business.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public Task<List<ProductModel>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<ProductModel?> GetByIdAsync(Guid id)
    {
        return _repository.GetByIdAsync(id);
    }

    public async Task CreateAsync(ProductModel product)
    {
        Validate(product);
        product.Id = Guid.NewGuid();
        await _repository.AddAsync(product);
    }

    public async Task UpdateAsync(ProductModel product)
    {
        Validate(product);

        var existing = await _repository.GetByIdAsync(product.Id);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Product '{product.Id}' was not found.");
        }

        await _repository.UpdateAsync(product);
    }

    public Task DeleteAsync(Guid id)
    {
        return _repository.DeleteAsync(id);
    }

    public Task<List<ProductModel>> SearchAsync(string keyword)
    {
        return _repository.SearchAsync(keyword);
    }

    private static void Validate(ProductModel product)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(product.Name))
        {
            errors.Add("Name is required.");
        }
        else if (product.Name.Trim().Length < 3)
        {
            errors.Add("Name must contain at least 3 characters.");
        }

        if (product.Price <= 0)
        {
            errors.Add("Price must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(product.Category))
        {
            errors.Add("Category is required.");
        }

        if (product.Description is { Length: > 500 })
        {
            errors.Add("Description cannot exceed 500 characters.");
        }

        if (errors.Count > 0)
        {
            throw new ProductValidationException(errors);
        }
    }
}
