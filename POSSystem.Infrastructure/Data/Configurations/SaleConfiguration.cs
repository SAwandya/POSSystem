using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(s => s.SubTotal)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.DiscountAmount)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.TaxAmount)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.GrandTotal)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.AmountPaid)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.ChangeAmount)
            .HasPrecision(18, 2);
        
        builder.HasIndex(s => s.InvoiceNumber).IsUnique();
        builder.HasIndex(s => s.SaleDate);
        
        // Relationships
        builder.HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(s => s.User)
            .WithMany(u => u.Sales)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(s => s.SaleItems)
            .WithOne(si => si.Sale)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
