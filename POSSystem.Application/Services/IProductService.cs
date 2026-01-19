using POSSystem.Application.DTOs;

namespace POSSystem.Application.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync();
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto?> GetProductByBarcodeAsync(string barcode);
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<bool> UpdateProductAsync(UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int id);
    Task<bool> UpdateStockAsync(int productId, decimal quantity, string reason);
}
