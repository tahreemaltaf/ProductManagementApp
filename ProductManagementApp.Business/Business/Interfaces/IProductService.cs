using ProductManagementApp.Models;

namespace ProductManagementApp.Business.Interfaces;

public interface IProductService
{
    Task<List<ProductModel>> GetAllAsync();

    Task<ProductModel?> GetByIdAsync(Guid id);

    Task CreateAsync(ProductModel product);

    Task UpdateAsync(ProductModel product);

    Task DeleteAsync(Guid id);

    Task<List<ProductModel>> SearchAsync(string keyword);
}
