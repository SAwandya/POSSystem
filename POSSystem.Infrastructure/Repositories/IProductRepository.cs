using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByBarcodeAsync(string barcode);
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
    Task<IEnumerable<Product>> GetLowStockProductsAsync();
    Task<Product?> GetProductWithInventoryAsync(int productId);
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
}
