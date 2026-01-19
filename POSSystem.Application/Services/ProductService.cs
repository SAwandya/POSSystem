using POSSystem.Application.DTOs;
using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Repositories;

namespace POSSystem.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
    {
        var products = await _unitOfWork.Products.GetActiveProductsAsync();
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        var products = await _unitOfWork.Products.SearchProductsAsync(searchTerm);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetProductWithInventoryAsync(id);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<ProductDto?> GetProductByBarcodeAsync(string barcode)
    {
        var product = await _unitOfWork.Products.GetByBarcodeAsync(barcode);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
    {
        var products = await _unitOfWork.Products.GetLowStockProductsAsync();
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        // Get first available SubCategoryId if not provided or invalid
        int? subCatId = dto.SubCatId;
        
        if (subCatId == null || subCatId <= 0)
        {
            // Try to get the first sub category
            var allSubCats = await _unitOfWork.Repository<SubCategory>().GetAllAsync();
            var firstSubCat = allSubCats.FirstOrDefault();
            subCatId = firstSubCat?.SubCatId;
        }

        // Create product
        var product = new Product
        {
            Name = dto.Name,
            Barcode = dto.Barcode,
            Description = dto.Description,
            SubCatId = subCatId, // Can be null - database allows it
            UnitMeasure = string.IsNullOrWhiteSpace(dto.UnitMeasure) ? "pcs" : dto.UnitMeasure,
            AlertQty = dto.AlertQty,
            IsActive = true
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Create inventory record
        var inventory = new Inventory
        {
            ProductId = product.ProductId,
            Quantity = dto.InitialQuantity,
            SellingPrice = dto.SellingPrice,
            AverageCost = 0
        };

        await _unitOfWork.Repository<Inventory>().AddAsync(inventory);
        await _unitOfWork.SaveChangesAsync();

        // Reload product with inventory
        var createdProduct = await _unitOfWork.Products.GetProductWithInventoryAsync(product.ProductId);
        return MapToDto(createdProduct!);
    }

    public async Task<bool> UpdateProductAsync(UpdateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product == null) return false;

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.AlertQty = dto.AlertQty;
        product.IsActive = dto.IsActive;

        _unitOfWork.Products.Update(product);

        // Update inventory selling price
        var inventory = await _unitOfWork.Repository<Inventory>()
            .FirstOrDefaultAsync(i => i.ProductId == dto.ProductId);
        
        if (inventory != null)
        {
            inventory.SellingPrice = dto.SellingPrice;
            _unitOfWork.Repository<Inventory>().Update(inventory);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return false;

        product.IsActive = false;
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStockAsync(int productId, decimal quantity, string reason)
    {
        var inventory = await _unitOfWork.Repository<Inventory>()
            .FirstOrDefaultAsync(i => i.ProductId == productId);
        
        if (inventory == null) return false;

        inventory.Quantity = quantity;
        _unitOfWork.Repository<Inventory>().Update(inventory);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Barcode = product.Barcode,
            Description = product.Description,
            Category = product.SubCategory?.Category?.Name ?? "Uncategorized",
            SubCategory = product.SubCategory?.Name ?? "Uncategorized",
            SellingPrice = product.Inventory?.SellingPrice ?? 0,
            Quantity = product.Inventory?.Quantity ?? 0,
            AlertQty = product.AlertQty,
            IsActive = product.IsActive,
            IsLowStock = product.Inventory != null && product.Inventory.Quantity <= product.AlertQty
        };
    }
}
