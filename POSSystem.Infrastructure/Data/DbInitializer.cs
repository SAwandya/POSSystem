using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Data;

namespace POSSystem.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedDataAsync(POSDbContext context)
    {
        // Check if database is already seeded
        if (context.Users.Any())
        {
            return; // DB has been seeded
        }

        // Create Admin User
        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = "admin123", // In production, this should be hashed
            FullName = "System Administrator",
            Role = UserRole.Admin,
            IsActive = true
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        // Create Permissions
        var permissions = new List<Permission>
        {
            new Permission { Slug = "pos.access", Name = "Access POS", ModuleGroup = "POS" },
            new Permission { Slug = "inventory.view", Name = "View Inventory", ModuleGroup = "Inventory" },
            new Permission { Slug = "inventory.edit", Name = "Adjust Stock", ModuleGroup = "Inventory" },
            new Permission { Slug = "drawer.open", Name = "Open Cash Drawer", ModuleGroup = "Cash Management" },
            new Permission { Slug = "drawer.close", Name = "Close Cash Drawer", ModuleGroup = "Cash Management" },
            new Permission { Slug = "reports.view", Name = "View Reports", ModuleGroup = "Reporting" }
        };

        context.Permissions.AddRange(permissions);
        await context.SaveChangesAsync();

        // Create Sample Categories
        var beveragesCategory = new Category { Name = "Beverages", Description = "All beverages" };
        var electronicsCategory = new Category { Name = "Electronics", Description = "Electronic items" };
        var groceriesCategory = new Category { Name = "Groceries", Description = "Grocery items" };
        var clothingCategory = new Category { Name = "Clothing", Description = "Clothing items" };
        var stationeryCategory = new Category { Name = "Stationery", Description = "Stationery items" };

        context.Categories.AddRange(new[] { beveragesCategory, electronicsCategory, groceriesCategory, clothingCategory, stationeryCategory });
        await context.SaveChangesAsync();

        // Create Subcategories
        var softDrinks = new SubCategory { CategoryId = beveragesCategory.CategoryId, Name = "Soft Drinks" };
        var mobiles = new SubCategory { CategoryId = electronicsCategory.CategoryId, Name = "Mobile Phones" };
        var dairy = new SubCategory { CategoryId = groceriesCategory.CategoryId, Name = "Dairy Products" };
        var menswear = new SubCategory { CategoryId = clothingCategory.CategoryId, Name = "Menswear" };
        var notebooks = new SubCategory { CategoryId = stationeryCategory.CategoryId, Name = "Notebooks" };

        context.SubCategories.AddRange(new[] { softDrinks, mobiles, dairy, menswear, notebooks });
        await context.SaveChangesAsync();

        // Create Sample Products
        var products = new List<Product>
        {
            new Product { Name = "Cola 500ml", Barcode = "123456789", SubCatId = softDrinks.SubCatId, UnitMeasure = "pcs", AlertQty = 10, IsActive = true },
            new Product { Name = "Apple iPhone 14", Barcode = "987654321", SubCatId = mobiles.SubCatId, UnitMeasure = "pcs", AlertQty = 5, IsActive = true },
            new Product { Name = "Milk 1 Liter", Barcode = "111222333", SubCatId = dairy.SubCatId, UnitMeasure = "ltr", AlertQty = 20, IsActive = true },
            new Product { Name = "T-Shirt", Barcode = "444555666", SubCatId = menswear.SubCatId, UnitMeasure = "pcs", AlertQty = 15, IsActive = true },
            new Product { Name = "Notebook A4", Barcode = "777888999", SubCatId = notebooks.SubCatId, UnitMeasure = "pcs", AlertQty = 25, IsActive = true }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Create Inventory for Products
        var inventories = new List<Inventory>
        {
            new Inventory { ProductId = products[0].ProductId, Quantity = 100, SellingPrice = 1.50m, AverageCost = 1.00m },
            new Inventory { ProductId = products[1].ProductId, Quantity = 15, SellingPrice = 79999.00m, AverageCost = 70000.00m },
            new Inventory { ProductId = products[2].ProductId, Quantity = 50, SellingPrice = 60.00m, AverageCost = 45.00m },
            new Inventory { ProductId = products[3].ProductId, Quantity = 30, SellingPrice = 499.00m, AverageCost = 350.00m },
            new Inventory { ProductId = products[4].ProductId, Quantity = 100, SellingPrice = 45.00m, AverageCost = 30.00m }
        };

        context.Inventories.AddRange(inventories);
        await context.SaveChangesAsync();

        // Create a default open drawer session for the admin user
        var drawerSession = new DrawerSession
        {
            UserId = adminUser.UserId,
            StartTime = DateTime.Now,
            OpeningCash = 1000.00m,
            Status = SessionStatus.Open
        };

        context.DrawerSessions.Add(drawerSession);
        await context.SaveChangesAsync();
    }
}
