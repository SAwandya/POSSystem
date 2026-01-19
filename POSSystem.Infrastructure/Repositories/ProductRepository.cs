using Microsoft.EntityFrameworkCore;
using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Data;

namespace POSSystem.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(POSDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode)
    {
        return await _dbSet
            .Include(p => p.Inventory)
            .Include(p => p.SubCategory)
                .ThenInclude(sc => sc!.Category)
            .FirstOrDefaultAsync(p => p.Barcode == barcode);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _dbSet
            .Include(p => p.Inventory)
            .Include(p => p.SubCategory)
                .ThenInclude(sc => sc!.Category)
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        searchTerm = searchTerm.ToLower();
        return await _dbSet
            .Include(p => p.Inventory)
            .Include(p => p.SubCategory)
                .ThenInclude(sc => sc!.Category)
            .Where(p => p.IsActive && 
                       (p.Name.ToLower().Contains(searchTerm) || 
                        p.Barcode!.ToLower().Contains(searchTerm)))
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
    {
        return await _dbSet
            .Include(p => p.Inventory)
            .Where(p => p.IsActive && p.Inventory != null && p.Inventory.Quantity <= p.AlertQty)
            .ToListAsync();
    }

    public async Task<Product?> GetProductWithInventoryAsync(int productId)
    {
        return await _dbSet
            .Include(p => p.Inventory)
            .Include(p => p.SubCategory)
                .ThenInclude(sc => sc!.Category)
            .FirstOrDefaultAsync(p => p.ProductId == productId);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Inventory)
            .Include(p => p.SubCategory)
            .Where(p => p.IsActive && p.SubCategory != null && p.SubCategory.CategoryId == categoryId)
            .ToListAsync();
    }
}
