using Microsoft.EntityFrameworkCore;
using POSSystem.Domain.Entities;
using POSSystem.Domain.Enums;
using POSSystem.Infrastructure.Data;

namespace POSSystem.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(POSDbContext context)
    {
        await context.Database.MigrateAsync();

        // Seed Users
        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@possystem.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "System Administrator",
                Role = UserRole.Admin,
                Phone = "1234567890",
                IsActive = true
            };

            var cashierUser = new User
            {
                Username = "cashier",
                Email = "cashier@possystem.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("cashier123"),
                FullName = "Cashier User",
                Role = UserRole.Cashier,
                Phone = "0987654321",
                IsActive = true
            };

            var managerUser = new User
            {
                Username = "manager",
                Email = "manager@possystem.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                FullName = "Store Manager",
                Role = UserRole.Manager,
                Phone = "1122334455",
                IsActive = true
            };

            await context.Users.AddRangeAsync(adminUser, cashierUser, managerUser);
            await context.SaveChangesAsync();
        }

        // Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { Name = "Electronics", Description = "Electronic items and gadgets", IsActive = true },
                new Category { Name = "Groceries", Description = "Food and grocery items", IsActive = true },
                new Category { Name = "Clothing", Description = "Apparel and fashion items", IsActive = true },
                new Category { Name = "Stationery", Description = "Office and school supplies", IsActive = true },
                new Category { Name = "Beverages", Description = "Drinks and beverages", IsActive = true }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // Seed Tax Rates
        if (!await context.TaxRates.AnyAsync())
        {
            var taxRates = new[]
            {
                new TaxRate { Name = "Standard VAT", Rate = 0.18m, Description = "18% VAT", IsActive = true },
                new TaxRate { Name = "Reduced VAT", Rate = 0.05m, Description = "5% VAT", IsActive = true },
                new TaxRate { Name = "Zero Rated", Rate = 0.00m, Description = "0% VAT", IsActive = true }
            };

            await context.TaxRates.AddRangeAsync(taxRates);
            await context.SaveChangesAsync();
        }

        // Seed Sample Products
        if (!await context.Products.AnyAsync())
        {
            var electronics = await context.Categories.FirstAsync(c => c.Name == "Electronics");
            var groceries = await context.Categories.FirstAsync(c => c.Name == "Groceries");
            var beverages = await context.Categories.FirstAsync(c => c.Name == "Beverages");

            var products = new[]
            {
                new Product
                {
                    Name = "iPhone 14 Pro Max",
                    SKU = "ELEC-IPH-001",
                    Barcode = "1234567890123",
                    Price = 1299.99m,
                    CostPrice = 1000.00m,
                    CategoryId = electronics.Id,
                    CurrentStock = 15,
                    MinStockLevel = 5,
                    MaxStockLevel = 50,
                    TaxRate = 0.18m,
                    IsActive = true
                },
                new Product
                {
                    Name = "Samsung Galaxy S23",
                    SKU = "ELEC-SAM-001",
                    Barcode = "2234567890123",
                    Price = 999.99m,
                    CostPrice = 800.00m,
                    CategoryId = electronics.Id,
                    CurrentStock = 20,
                    MinStockLevel = 5,
                    MaxStockLevel = 50,
                    TaxRate = 0.18m,
                    IsActive = true
                },
                new Product
                {
                    Name = "Coca Cola 2L",
                    SKU = "BEV-COK-001",
                    Barcode = "3234567890123",
                    Price = 3.99m,
                    CostPrice = 2.50m,
                    CategoryId = beverages.Id,
                    CurrentStock = 100,
                    MinStockLevel = 20,
                    MaxStockLevel = 200,
                    TaxRate = 0.05m,
                    IsActive = true
                },
                new Product
                {
                    Name = "Rice 5Kg",
                    SKU = "GRO-RIC-001",
                    Barcode = "4234567890123",
                    Price = 15.99m,
                    CostPrice = 12.00m,
                    CategoryId = groceries.Id,
                    CurrentStock = 50,
                    MinStockLevel = 10,
                    MaxStockLevel = 100,
                    TaxRate = 0.00m,
                    IsActive = true
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        // Seed Walk-in Customer
        if (!await context.Customers.AnyAsync())
        {
            var walkInCustomer = new Customer
            {
                CustomerCode = "CUST-000",
                Name = "Walk-in Customer",
                Phone = "0000000000",
                IsActive = true
            };

            await context.Customers.AddAsync(walkInCustomer);
            await context.SaveChangesAsync();
        }
    }
}
