using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(p => p.Barcode)
            .HasMaxLength(100);
        
        builder.Property(p => p.Price)
            .HasPrecision(18, 2);
        
        builder.Property(p => p.CostPrice)
            .HasPrecision(18, 2);
        
        builder.Property(p => p.TaxRate)
            .HasPrecision(5, 2);
        
        builder.HasIndex(p => p.SKU).IsUnique();
        builder.HasIndex(p => p.Barcode);
        
        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(p => p.SaleItems)
            .WithOne(si => si.Product)
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(p => p.GRNItems)
            .WithOne(gi => gi.Product)
            .HasForeignKey(gi => gi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(p => p.InventoryTransactions)
            .WithOne(it => it.Product)
            .HasForeignKey(it => it.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Ignore computed property
        builder.Ignore(p => p.StockStatus);
    }
}
