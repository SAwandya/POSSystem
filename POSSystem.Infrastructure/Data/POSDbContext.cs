using Microsoft.EntityFrameworkCore;
using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Data;

public class POSDbContext : DbContext
{
    public POSDbContext(DbContextOptions<POSDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SubCategory> SubCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<ProductSupplier> ProductSuppliers { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<StockAdjustment> StockAdjustments { get; set; }
    public DbSet<AdjustmentItem> AdjustmentItems { get; set; }
    public DbSet<DrawerSession> DrawerSessions { get; set; }
    public DbSet<DrawerCashFlow> DrawerCashFlows { get; set; }
    public DbSet<GRN> GRNs { get; set; }
    public DbSet<GRNItem> GRNItems { get; set; }
    public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
    public DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SalesItem> SalesItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<SalesReturn> SalesReturns { get; set; }
    public DbSet<SalesReturnItem> SalesReturnItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100);
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Permission Configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.PermissionId);
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.ModuleGroup).HasColumnName("module_group").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");

            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // UserPermission Configuration (Many-to-Many)
        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.ToTable("user_permissions");
            entity.HasKey(e => new { e.UserId, e.PermissionId });
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ActivityLog Configuration
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("activity_logs");
            entity.HasKey(e => e.LogId);
            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Details).HasColumnName("details");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");

            entity.HasOne(e => e.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Category Configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
        });

        // SubCategory Configuration
        modelBuilder.Entity<SubCategory>(entity =>
        {
            entity.ToTable("sub_categories");
            entity.HasKey(e => e.SubCatId);
            entity.Property(e => e.SubCatId).HasColumnName("sub_cat_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SubCatId).HasColumnName("sub_cat_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Barcode).HasColumnName("barcode").HasMaxLength(50);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.UnitMeasure).HasColumnName("unit_measure").HasMaxLength(20);
            entity.Property(e => e.AlertQty).HasColumnName("alert_qty").HasDefaultValue(10);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.Barcode).IsUnique();

            entity.HasOne(e => e.SubCategory)
                .WithMany(sc => sc.Products)
                .HasForeignKey(e => e.SubCatId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Supplier Configuration
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");
            entity.HasKey(e => e.SupplierId);
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.ContactPerson).HasColumnName("contact_person").HasMaxLength(100);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.Address).HasColumnName("address");
        });

        // ProductSupplier Configuration (Many-to-Many)
        modelBuilder.Entity<ProductSupplier>(entity =>
        {
            entity.ToTable("product_suppliers");
            entity.HasKey(e => new { e.ProductId, e.SupplierId });
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(100);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.ProductSuppliers)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.ProductSuppliers)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Inventory Configuration
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("inventory");
            entity.HasKey(e => e.InventoryId);
            entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(10, 2).HasDefaultValue(0);
            entity.Property(e => e.AverageCost).HasColumnName("average_cost").HasPrecision(10, 2).HasDefaultValue(0);
            entity.Property(e => e.SellingPrice).HasColumnName("selling_price").HasPrecision(10, 2).HasDefaultValue(0);
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");

            entity.HasIndex(e => e.ProductId).IsUnique();

            entity.HasOne(e => e.Product)
                .WithOne(p => p.Inventory)
                .HasForeignKey<Inventory>(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StockAdjustment Configuration
        modelBuilder.Entity<StockAdjustment>(entity =>
        {
            entity.ToTable("stock_adjustments");
            entity.HasKey(e => e.AdjustmentId);
            entity.Property(e => e.AdjustmentId).HasColumnName("adjustment_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ReferenceNo).HasColumnName("reference_no").HasMaxLength(50);
            entity.Property(e => e.AdjustmentDate).HasColumnName("adjustment_date");
            entity.Property(e => e.ReasonType).HasColumnName("reason_type").HasConversion<string>();
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(e => e.User)
                .WithMany(u => u.StockAdjustments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AdjustmentItem Configuration
        modelBuilder.Entity<AdjustmentItem>(entity =>
        {
            entity.ToTable("adjustment_items");
            entity.HasKey(e => e.AdjItemId);
            entity.Property(e => e.AdjItemId).HasColumnName("adj_item_id");
            entity.Property(e => e.AdjustmentId).HasColumnName("adjustment_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SystemQty).HasColumnName("system_qty").HasPrecision(10, 2);
            entity.Property(e => e.PhysicalQty).HasColumnName("physical_qty").HasPrecision(10, 2);
            entity.Property(e => e.DifferenceQty).HasColumnName("difference_qty").HasPrecision(10, 2);

            entity.HasOne(e => e.StockAdjustment)
                .WithMany(sa => sa.AdjustmentItems)
                .HasForeignKey(e => e.AdjustmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.AdjustmentItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DrawerSession Configuration
        modelBuilder.Entity<DrawerSession>(entity =>
        {
            entity.ToTable("drawer_sessions");
            entity.HasKey(e => e.SessionId);
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.OpeningCash).HasColumnName("opening_cash").HasPrecision(15, 2).HasDefaultValue(0);
            entity.Property(e => e.ClosingCashSystem).HasColumnName("closing_cash_system").HasPrecision(15, 2).HasDefaultValue(0);
            entity.Property(e => e.ClosingCashActual).HasColumnName("closing_cash_actual").HasPrecision(15, 2);
            entity.Property(e => e.Variance).HasColumnName("variance").HasPrecision(15, 2);
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.Remarks).HasColumnName("remarks");

            entity.HasOne(e => e.User)
                .WithMany(u => u.DrawerSessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DrawerCashFlow Configuration
        modelBuilder.Entity<DrawerCashFlow>(entity =>
        {
            entity.ToTable("drawer_cash_flows");
            entity.HasKey(e => e.FlowId);
            entity.Property(e => e.FlowId).HasColumnName("flow_id");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(15, 2);
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>();
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(255);
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");

            entity.HasOne(e => e.DrawerSession)
                .WithMany(ds => ds.DrawerCashFlows)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GRN Configuration
        modelBuilder.Entity<GRN>(entity =>
        {
            entity.ToTable("grn");
            entity.HasKey(e => e.GrnId);
            entity.Property(e => e.GrnId).HasColumnName("grn_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ReferenceNo).HasColumnName("reference_no").HasMaxLength(50);
            entity.Property(e => e.ReceivedDate).HasColumnName("received_date");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasPrecision(15, 2);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.GRNs)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // GRNItem Configuration
        modelBuilder.Entity<GRNItem>(entity =>
        {
            entity.ToTable("grn_items");
            entity.HasKey(e => e.GrnItemId);
            entity.Property(e => e.GrnItemId).HasColumnName("grn_item_id");
            entity.Property(e => e.GrnId).HasColumnName("grn_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(10, 2);
            entity.Property(e => e.UnitCost).HasColumnName("unit_cost").HasPrecision(15, 2);
            entity.Property(e => e.TotalCost).HasColumnName("total_cost").HasPrecision(15, 2).HasComputedColumnSql("(quantity * unit_cost)", stored: true);
            entity.Property(e => e.QuantityReturned).HasColumnName("quantity_returned").HasPrecision(10, 2).HasDefaultValue(0);

            entity.HasOne(e => e.GRN)
                .WithMany(g => g.GRNItems)
                .HasForeignKey(e => e.GrnId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.GRNItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Customer Configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.PointsBalance).HasColumnName("points_balance").HasDefaultValue(0);
        });

        // Sale Configuration
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.ToTable("sales");
            entity.HasKey(e => e.SaleId);
            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.SaleDate).HasColumnName("sale_date");
            entity.Property(e => e.SubTotal).HasColumnName("sub_total").HasPrecision(15, 2);
            entity.Property(e => e.TaxAmount).HasColumnName("tax_amount").HasPrecision(15, 2);
            entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount").HasPrecision(15, 2);
            entity.Property(e => e.GrandTotal).HasColumnName("grand_total").HasPrecision(15, 2);
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status").HasConversion<string>();

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Sales)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DrawerSession)
                .WithMany(ds => ds.Sales)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SalesItem Configuration
        modelBuilder.Entity<SalesItem>(entity =>
        {
            entity.ToTable("sales_items");
            entity.HasKey(e => e.ItemId);
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(10, 2);
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasPrecision(15, 2);
            entity.Property(e => e.TotalPrice).HasColumnName("total_price").HasPrecision(15, 2).HasComputedColumnSql("(quantity * unit_price)", stored: true);
            entity.Property(e => e.QuantityReturned).HasColumnName("quantity_returned").HasPrecision(10, 2).HasDefaultValue(0);

            entity.HasOne(e => e.Sale)
                .WithMany(s => s.SalesItems)
                .HasForeignKey(e => e.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.SalesItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(15, 2);
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasConversion<string>();
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");

            entity.HasOne(e => e.Sale)
                .WithMany(s => s.Payments)
                .HasForeignKey(e => e.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SalesReturn Configuration
        modelBuilder.Entity<SalesReturn>(entity =>
        {
            entity.ToTable("sales_returns");
            entity.HasKey(e => e.ReturnId);
            entity.Property(e => e.ReturnId).HasColumnName("return_id");
            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.ReturnDate).HasColumnName("return_date");
            entity.Property(e => e.TotalRefund).HasColumnName("total_refund").HasPrecision(15, 2).HasDefaultValue(0);
            entity.Property(e => e.Reason).HasColumnName("reason");

            entity.HasOne(e => e.Sale)
                .WithMany(s => s.SalesReturns)
                .HasForeignKey(e => e.SaleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DrawerSession)
                .WithMany(ds => ds.SalesReturns)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SalesReturnItem Configuration
        modelBuilder.Entity<SalesReturnItem>(entity =>
        {
            entity.ToTable("sales_return_items");
            entity.HasKey(e => e.ReturnItemId);
            entity.Property(e => e.ReturnItemId).HasColumnName("return_item_id");
            entity.Property(e => e.ReturnId).HasColumnName("return_id");
            entity.Property(e => e.SaleItemId).HasColumnName("sale_item_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(10, 2);
            entity.Property(e => e.RefundAmount).HasColumnName("refund_amount").HasPrecision(15, 2);
            entity.Property(e => e.ConditionStatus).HasColumnName("condition_status").HasConversion<string>();

            entity.HasOne(e => e.SalesReturn)
                .WithMany(sr => sr.SalesReturnItems)
                .HasForeignKey(e => e.ReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SalesItem)
                .WithMany(si => si.SalesReturnItems)
                .HasForeignKey(e => e.SaleItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PurchaseReturn Configuration
        modelBuilder.Entity<PurchaseReturn>(entity =>
        {
            entity.ToTable("purchase_returns");
            entity.HasKey(e => e.PrId);
            entity.Property(e => e.PrId).HasColumnName("pr_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ReturnDate).HasColumnName("return_date");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.TotalRefundAmount).HasColumnName("total_refund_amount").HasPrecision(15, 2).HasDefaultValue(0);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.PurchaseReturns)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PurchaseReturnItem Configuration
        modelBuilder.Entity<PurchaseReturnItem>(entity =>
        {
            entity.ToTable("purchase_return_items");
            entity.HasKey(e => e.PrItemId);
            entity.Property(e => e.PrItemId).HasColumnName("pr_item_id");
            entity.Property(e => e.PrId).HasColumnName("pr_id");
            entity.Property(e => e.GrnItemId).HasColumnName("grn_item_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(10, 2);
            entity.Property(e => e.RefundUnitCost).HasColumnName("refund_unit_cost").HasPrecision(15, 2);
            entity.Property(e => e.TotalRefund).HasColumnName("total_refund").HasPrecision(15, 2).HasComputedColumnSql("(quantity * refund_unit_cost)", stored: true);
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(255);

            entity.HasOne(e => e.PurchaseReturn)
                .WithMany(pr => pr.PurchaseReturnItems)
                .HasForeignKey(e => e.PrId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.GRNItem)
                .WithMany(gi => gi.PurchaseReturnItems)
                .HasForeignKey(e => e.GrnItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
