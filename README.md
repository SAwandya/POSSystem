# POS System Pro - Setup Guide

## 🚀 Quick Start Guide

### Prerequisites
1. **MySQL Server 8.0+** installed and running
2. **.NET 9 SDK** installed
3. **Visual Studio 2022** or VS Code

### Database Setup

#### Step 1: Create Database
Open MySQL Workbench or command line and run:

```sql
CREATE DATABASE IF NOT EXISTS pos_system_pro;
USE pos_system_pro;
```

#### Step 2: Run the SQL Script
Execute the complete SQL script provided (`database_schema.sql`) to create all tables.

#### Step 3: Configure Connection String
Open `POSSystem.UI\appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=pos_system_pro;User=root;Password=YOUR_MYSQL_PASSWORD;Port=3306;CharSet=utf8mb4;"
  }
}
```

**Important:** Replace `YOUR_MYSQL_PASSWORD` with your actual MySQL root password.

### Running the Application

1. **Build the Solution**
   ```bash
   dotnet build
   ```

2. **Run the Application**
   ```bash
   dotnet run --project POSSystem.UI\POSSystem.UI.csproj
   ```

   Or press **F5** in Visual Studio

3. **Login Credentials**
   - **Admin/Manager:** username: `admin`, password: `admin`
   - **Billing User:** username: `bill`, password: `123`

### Features Implemented

#### ✅ Inventory Management
- View all products with stock levels
- Add/Edit/Delete products
- Low stock alerts
- Product search by name or barcode
- Category-based filtering

#### ✅ Sales & Billing
- Complete POS billing interface
- Real-time inventory updates
- Multiple payment methods (Cash, Card, UPI, Credit)
- Tax calculation (18% GST)
- Discount management
- Two bill formats:
  - Relaword Format (80mm thermal printer)
  - Standard A4 format
- Invoice generation
- Change calculation

#### ✅ Database Integration
- Entity Framework Core 9.0
- MySQL/MariaDB support
- Repository pattern
- Unit of Work pattern
- Transaction management
- Automatic database creation
- Sample data seeding

### Project Structure

```
POSSystem/
├── POSSystem.Domain/           # Domain entities
│   └── Entities/               # All database entities
├── POSSystem.Infrastructure/   # Data access layer
│   ├── Data/                   # DbContext & migrations
│   └── Repositories/           # Repository implementations
├── POSSystem.Application/      # Business logic layer
│   ├── DTOs/                   # Data Transfer Objects
│   └── Services/               # Service implementations
└── POSSystem.UI/               # WPF User Interface
    ├── Views/                  # All XAML views
    │   ├── Dashboard/
    │   ├── Sales/
    │   ├── Inventory/
    │   └── ...
    └── appsettings.json        # Configuration
```

### Testing the System

1. **Test Product Management:**
   - Navigate to Dashboard
   - Click "Inventory" button
   - Products are loaded from database
   - Try adding/editing products

2. **Test Sales:**
   - Login with `bill / 123`
   - BillingPage opens
   - Products are loaded from database
   - Add items to cart
   - Process payment
   - Verify inventory is updated
   - Sale is saved to database

3. **Verify Database:**
   ```sql
   -- Check products
   SELECT * FROM products;
   
   -- Check inventory
   SELECT p.name, i.quantity, i.selling_price 
   FROM products p 
   JOIN inventory i ON p.product_id = i.product_id;
   
   -- Check sales
   SELECT * FROM sales;
   SELECT * FROM sales_items;
   ```

### Common Issues & Solutions

#### Issue: "Database connection failed"
**Solution:** 
- Ensure MySQL server is running
- Check connection string in appsettings.json
- Verify username/password
- Ensure database `pos_system_pro` exists

#### Issue: "Could not find appsettings.json"
**Solution:**
- Build the project (appsettings.json is copied to output directory)
- Or manually copy appsettings.json to bin/Debug/net9.0-windows/

#### Issue: "Table doesn't exist"
**Solution:**
- Run the complete SQL script again
- Or let the application create tables (EnsureCreated)

### Next Steps (Future Enhancements)

- [ ] User authentication with password hashing
- [ ] GRN (Goods Receipt Note) integration
- [ ] Reports & Analytics
- [ ] Customer management
- [ ] Stock adjustments
- [ ] Purchase returns & sales returns
- [ ] Drawer management
- [ ] Permissions & role-based access

### Tech Stack

- **Framework:** .NET 9
- **UI:** WPF with Material Design
- **Database:** MySQL 8.0+ / MariaDB
- **ORM:** Entity Framework Core 9.0
- **Architecture:** Clean Architecture (Domain, Application, Infrastructure, UI)
- **Patterns:** Repository, Unit of Work, Dependency Injection

### Support

For issues or questions:
1. Check the error logs in the application
2. Verify database connection
3. Check that all NuGet packages are restored
4. Ensure .NET 9 SDK is installed

---

**Built with ❤️ for efficient retail management**
