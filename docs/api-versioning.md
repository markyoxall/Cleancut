# API Versioning Strategy - CleanCut API

## Overview

This document outlines the API versioning strategy implemented in the CleanCut API to ensure backward compatibility while enabling new feature development.

## Versioning Approach

### **Route-Based Versioning**
We use **URL path versioning** for clear, explicit version identification:

- **Version 1**: `/api/v1/products`
- **Version 2**: `/api/v2/products`

## Version Details

### **Version 1.0 (Current)**
**Route**: `/api/v1/products`
**Controller**: `CleanCut.API.Controllers.ProductsController`

**Features**:
- ? Basic CRUD operations
- ? Simple error responses
- ? Standard JSON responses
- ? Clean Architecture implementation

**Endpoints**:
```
GET    /api/v1/products/{id}           - Get product by ID
GET    /api/v1/products/user/{userId}  - Get products by user
POST   /api/v1/products                - Create product
PUT    /api/v1/products/{id}           - Update product
```

### **Version 2.0 (Enhanced)**
**Route**: `/api/v2/products`
**Controller**: `CleanCut.API.Controllers.V2.ProductsController`

**New Features**:
- ? **Enhanced response format** with metadata
- ? **Pagination support** for list endpoints
- ? **Improved error handling** with error codes
- ? **Request tracking** with correlation IDs
- ? **Timestamps** on all responses
- ? **New statistics endpoint**

**Endpoints**:
```
GET    /api/v2/products/{id}           - Enhanced product retrieval
GET    /api/v2/products/user/{userId}  - Paginated user products
POST   /api/v2/products                - Enhanced product creation
PUT    /api/v2/products/{id}           - Enhanced product update
GET    /api/v2/products/statistics     - Product statistics (NEW)
```

## Response Format Differences

### **Version 1 Response Format**
```json
{
  "id": "guid",
  "name": "Product Name",
  "description": "Description",
  "price": 29.99,
  "isAvailable": true,
  "userId": "guid",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

### **Version 2 Response Format**
```json
{
  "data": {
    "id": "guid",
    "name": "Product Name",
    "description": "Description",
    "price": 29.99,
    "isAvailable": true,
    "userId": "guid",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "apiVersion": "2.0",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## Pagination (V2 Only)

### **Request**
```
GET /api/v2/products/user/{userId}?page=1&pageSize=10
```

### **Response**
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 100,
    "totalPages": 10
  },
  "apiVersion": "2.0",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## Error Handling Differences

### **Version 1 Errors**
```json
"Product with ID {id} not found"
```

### **Version 2 Errors**
```json
{
  "error": "ProductNotFound",
  "message": "Product with ID {id} not found",
  "requestId": "trace-id",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## New Features in V2

### **Product Statistics Endpoint**
```
GET /api/v2/products/statistics
```

**Response**:
```json
{
  "data": {
    "totalProducts": 100,
    "availableProducts": 85,
    "unavailableProducts": 15,
    "averagePrice": 299.99,
    "lastUpdated": "2024-01-01T11:30:00Z"
  },
  "message": "Product statistics retrieved successfully",
  "apiVersion": "2.0",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## Implementation Guidelines

### **Adding New Versions**

1. **Create new controller namespace**:
   ```
   CleanCut.API.Controllers.V3.ProductsController
   ```

2. **Use new route**:
   ```csharp
   [Route("api/v3/[controller]")]
   ```

3. **Implement new features** while maintaining existing functionality

4. **Update Swagger configuration**:
   ```csharp
   options.SwaggerEndpoint("/openapi/v3.json", "CleanCut API v3");
   ```

### **Backward Compatibility Rules**

- ? **Never break existing endpoints**
- ? **Maintain response contracts**
- ? **Support old versions for reasonable time**
- ? **Document deprecation timeline**
- ? **Provide migration guides**

### **Deprecation Strategy**

1. **Announce deprecation** in response headers:
   ```
   X-API-Deprecated: true
   X-API-Sunset: 2025-12-31
   ```

2. **Document timeline** in API documentation

3. **Provide migration path** to newer versions

4. **Monitor usage** before removal

## Client Usage Examples

### **JavaScript/Fetch**
```javascript
// Version 1
const response = await fetch('/api/v1/products/123');
const product = await response.json();

// Version 2
const response = await fetch('/api/v2/products/123');
const result = await response.json();
const product = result.data;
```

### **C# HttpClient**
```csharp
// Version 1
var product = await httpClient.GetFromJsonAsync<ProductDto>("/api/v1/products/123");

// Version 2
var result = await httpClient.GetFromJsonAsync<V2Response<ProductDto>>("/api/v2/products/123");
var product = result.Data;
```

## Swagger Documentation

Both versions are documented in Swagger UI:
- **V1**: Available at `/swagger` with v1 endpoints
- **V2**: Available at `/swagger` with v2 endpoints

## Testing Strategy

### **Version Compatibility Tests**
```csharp
[Test]
public async Task V1_GetProduct_ReturnsDirectObject()
{
    var response = await client.GetAsync("/api/v1/products/123");
    var product = await response.Content.ReadFromJsonAsync<ProductDto>();
    Assert.NotNull(product.Name);
}

[Test]
public async Task V2_GetProduct_ReturnsWrappedObject()
{
    var response = await client.GetAsync("/api/v2/products/123");
    var result = await response.Content.ReadFromJsonAsync<V2Response<ProductDto>>();
    Assert.Equal("2.0", result.ApiVersion);
    Assert.NotNull(result.Data.Name);
}
```

## Benefits of This Approach

1. ? **Clear Version Identification**: URL clearly shows version
2. ? **No Breaking Changes**: V1 clients continue working
3. ? **Easy Testing**: Can test both versions independently
4. ? **Gradual Migration**: Clients can migrate at their own pace
5. ? **Feature Innovation**: V2 introduces new capabilities
6. ? **Documentation**: Both versions fully documented

This versioning strategy ensures that the CleanCut API can evolve while maintaining backward compatibility and providing clear migration paths for API consumers.