# Database Seeding Management

This document explains how to work with the enhanced database seeding in the CleanCut project.

## Enhanced Seeding Features

The DatabaseSeeder has been enhanced with:

### **?? Comprehensive Test Data**
- **50+ Users** from various companies and domains
- **200+ Products** across multiple categories
- **Realistic pricing** with variations
- **Mixed availability status** (85% available, 15% unavailable)

### **?? Test-Friendly GUIDs**
- **Known User ID**: `11111111-1111-1111-1111-111111111111` (John Doe)
- **Known Product ID**: `b6cc1306-75ea-45f0-902e-6cdf34260651` (Gaming Laptop)

### **?? Product Categories**
- **Technology**: Laptops, mice, keyboards, monitors, etc.
- **Office & Productivity**: Standing desks, monitor arms, desk lamps
- **Smart Home & IoT**: Smart speakers, thermostats, security cameras
- **Gaming & Entertainment**: Gaming chairs, headsets, streaming equipment
- **Mobile & Accessories**: Phone cases, wireless earbuds, power banks
- **Networking**: WiFi routers, mesh systems, network switches
- **Software & Digital**: Antivirus, photo editors, productivity suites

## How to Reset and Reseed the Database

### **Option 1: Delete Database (Recommended for Development)**

1. **Stop the API** if it's running
2. **Delete the database** (if using LocalDB/SQL Server):
   ```bash
   # If using Entity Framework migrations
   dotnet ef database drop --project src/Infrastructure/CleanCut.Infrastructure.Data --startup-project src/Presentation/CleanCut.API --force
   ```

3. **Restart the API** - it will automatically recreate and seed the database

### **Option 2: Using Entity Framework Commands**

```bash
# Navigate to the solution root directory
cd "D:\.NET STUDY\CleanCut"

# Drop the database
dotnet ef database drop --project src/Infrastructure/CleanCut.Infrastructure.Data --startup-project src/Presentation/CleanCut.API --force

# Update/create the database (this will automatically trigger seeding)
dotnet ef database update --project src/Infrastructure/CleanCut.Infrastructure.Data --startup-project src/Presentation/CleanCut.API
```

### **Option 3: Manual Database Reset (SQL Server)**

If you're using SQL Server Management Studio:

1. Connect to your SQL Server instance
2. Delete the CleanCut database
3. Start the API - it will recreate and seed automatically

## Verifying the Seeding

After reseeding, you can verify the data through:

### **1. API Endpoints**
- **GET** `https://localhost:7142/api/users` - Should return 50+ users
- **GET** `https://localhost:7142/api/v1/products` - Should return products for the first user
- **GET** `https://localhost:7142/api/v2/products?pageSize=20` - Should return paginated products

### **2. Database Direct Query**
```sql
-- Check user count
SELECT COUNT(*) as UserCount FROM Users;

-- Check product count
SELECT COUNT(*) as ProductCount FROM Products;

-- Check products by availability
SELECT IsAvailable, COUNT(*) as Count 
FROM Products 
GROUP BY IsAvailable;

-- Verify test GUIDs exist
SELECT * FROM Users WHERE Id = '11111111-1111-1111-1111-111111111111';
SELECT * FROM Products WHERE Id = 'b6cc1306-75ea-45f0-902e-6cdf34260651';
```

### **3. API Demo Page**
- Navigate to `/api-demo` in your Blazor app
- Test pagination with different page sizes
- Verify the test GUIDs work in the search forms

## Pagination Testing Benefits

With 200+ products, you can now effectively test:

### **V1 API Pagination Simulation**
- Products are distributed across 50+ users
- Each user typically has 3-6 products
- Good for testing user-specific product lists

### **V2 API Pagination**
- Page sizes of 5, 10, 20, 50 will show multiple pages
- Test edge cases like page 1, last page, beyond last page
- Verify total counts and page calculations

### **Statistics Testing**
- Real product counts for statistics endpoints
- Mix of available/unavailable products
- Realistic price ranges for average calculations

## Seeding Configuration

The seeder is configured to:

- **Skip seeding** if data already exists (checks for any users)
- **Use consistent random seed (42)** for reproducible test data
- **Apply 85% availability rate** for realistic scenarios
- **Vary prices by ±20%** for each product instance
- **Create 2-4 variants** of each base product

## Troubleshooting

### **Seeding Fails**
- Check connection string in `appsettings.json`
- Ensure database server is running
- Verify permissions for database creation

### **Partial Seeding**
- Check API startup logs for seeding status
- Look for exception messages in the console
- Verify email uniqueness constraints aren't violated

### **Performance Issues**
- The seeding process creates 250+ records, which may take 5-10 seconds
- This only happens once when the database is empty
- Subsequent API starts will skip seeding

## Next Steps

With comprehensive seed data in place, you can now:

1. **Complete missing API implementations** (UPDATE operations, DELETE operations)
2. **Implement real statistics calculations** instead of mock data
3. **Add advanced filtering and search capabilities**
4. **Test performance with realistic data volumes**
5. **Implement caching strategies**

The enhanced seeding provides a solid foundation for developing and testing all API features with realistic, diverse data!