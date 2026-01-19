# ?? Quick Start Guide - POS System

## ? 5-Minute Setup

### 1. Configure Database (1 min)
Edit `POSSystem.UI\appsettings.json`:
```json
"DefaultConnection": "Server=localhost;Database=pos_system_pro;User=root;Password=YOUR_PASSWORD;Port=3306;CharSet=utf8mb4;"
```

### 2. Create Database (1 min)
```sql
CREATE DATABASE pos_system_pro;
```
Run your SQL script to create tables.

### 3. Run Application (1 min)
```bash
dotnet run --project POSSystem.UI\POSSystem.UI.csproj
```
Or press **F5** in Visual Studio

### 4. Login (1 sec)
- **Billing:** `bill` / `123`
- **Admin:** `admin` / `admin`

### 5. Test Sale (1 min)
1. Add products to cart
2. Click "Process Payment"
3. Enter amount
4. ? Done! Sale saved to database

---

## ?? What's Integrated

| Module | Status | Database Tables Used |
|--------|--------|---------------------|
| **Products** | ? LIVE | `products`, `inventory` |
| **Sales** | ? LIVE | `sales`, `sales_items`, `payments` |
| **Inventory Updates** | ? AUTO | `inventory` |
| **Sessions** | ? ACTIVE | `drawer_sessions` |

---

## ?? Key Features

? **Real Database** - MySQL with EF Core  
? **Auto-Inventory Update** - Stock reduces on sale  
? **Transaction Safe** - Rollback on errors  
? **Clean Architecture** - Domain ? Application ? Infrastructure ? UI  
? **Dependency Injection** - Properly configured  
? **Sample Data** - Auto-seeded on first run  

---

## ?? Quick Test

```sql
-- After making a sale, check:
SELECT * FROM sales ORDER BY sale_id DESC LIMIT 1;
SELECT * FROM sales_items WHERE sale_id = (SELECT MAX(sale_id) FROM sales);
SELECT product_id, quantity FROM inventory WHERE product_id IN (1,2,3);
```

---

## ?? If Something Goes Wrong

1. **Check MySQL is running**
2. **Verify connection string**
3. **Check console for error messages**
4. **Database created? Run SQL script**
5. **Still issues? Check `IMPLEMENTATION_COMPLETE.md`**

---

## ?? File Structure

```
POSSystem/
??? POSSystem.Domain/           ? Entities
??? POSSystem.Infrastructure/   ? Database + Repositories
??? POSSystem.Application/      ? Services + DTOs
??? POSSystem.UI/              ? WPF Interface
    ??? appsettings.json       ?? Configure here!
```

---

## ?? Success Indicators

? Products load from database  
? Stock shows real numbers  
? Sales save to MySQL  
? Inventory updates automatically  
? No errors in console  

---

**Your system is ready! Happy selling! ??**
