# CleanCut MVC WebApp - Startup Guide

## ?? **Getting Started**

The MVC WebApp (`CleanCut.WebApp`) now has **full authentication** and **all "User" references updated to "Customer"**.

### ? **Fixed Issues:**

1. **?? Authentication Integration**: Added complete OAuth 2.0 client credentials flow
2. **?? User ? Customer**: Updated all references from "User" to "Customer"
3. **?? Navigation**: Fixed layout navigation to use `CustomersController`
4. **?? Views**: Updated all views to use correct terminology
5. **?? IdentityServer**: Added `CleanCutWebApp` client to IdentityServer configuration

### ?? **Startup Sequence** (Required)

To use the MVC WebApp, you need to start these in order:

#### 1. **Start IdentityServer** (Port 5001)
```bash
cd src/Infrastructure/CleanCut.Infranstructure.Identity
dotnet run
```
**URL**: `https://localhost:5001`

#### 2. **Start CleanCut API** (Port 7142)
```bash
cd src/Presentation/CleanCut.API
dotnet run
```
**URL**: `https://localhost:7142`

#### 3. **Start MVC WebApp** (Port 7078)
```bash
cd src/Presentation/CleanCut.WebApp
dotnet run
```
**URL**: `https://localhost:7078`

### ?? **Authentication Configuration**

The MVC WebApp is configured with:
- **Client ID**: `CleanCutWebApp`
- **Client Secret**: `WebAppSecret2024!`
- **Grant Type**: `client_credentials`
- **Scope**: `CleanCutAPI`

### ?? **Features Available**

#### **Customer Management** (`/Customers`)
- ? View all customers
- ? Create new customers  
- ? Edit customer details
- ? Delete customers
- ? Search and filter

#### **Product Management** (`/Products`)
- ? View all products
- ? Create new products
- ? Edit product details  
- ? Delete products
- ? Filter by customer
- ? Search by name/description

### ??? **Updated Components**

#### **Controllers**
- ? `CustomersController` - Full CRUD with authentication
- ? `ProductsController` - Full CRUD with customer filtering
- ? `HomeController` - Updated references

#### **Services**  
- ? `TokenService` - OAuth 2.0 client credentials flow
- ? `AuthenticatedHttpMessageHandler` - Automatic token injection
- ? `CustomerApiService` - Authenticated API calls
- ? `ProductApiService` - Authenticated API calls

#### **Views**
- ? Updated navigation (`_Layout.cshtml`)
- ? Home page (`Index.cshtml`) 
- ? All Customer views
- ? All Product views
- ? Fixed parameter names (`customerId` vs `userId`)

#### **Models**
- ? `CustomerViewModels` - Updated validation messages
- ? `ProductViewModels` - Changed "user" to "customer"

### ?? **Testing**

1. **Start all 3 projects** in the order shown above
2. **Navigate** to `https://localhost:7078`
3. **Click** "Customer Management" to test customer features
4. **Click** "Product Management" to test product features

### ?? **Troubleshooting**

#### **No data returned?**
- ? Check IdentityServer is running on port 5001
- ? Check API is running on port 7142  
- ? Check browser network tab for authentication errors

#### **401 Unauthorized?**
- ? Verify IdentityServer client configuration
- ? Check appsettings.json configuration
- ? Look at application logs for token issues

#### **404 Not Found?**
- ? Verify all controllers use `Customers` not `Users`
- ? Check navigation URLs in `_Layout.cshtml`

### ?? **Architecture**

The MVC WebApp now follows the same authentication pattern as the BlazorWebApp:

```
MVC WebApp ? TokenService ? IdentityServer ? Access Token ? API
```

All API calls are authenticated using Bearer tokens obtained via OAuth 2.0 client credentials flow.

### ? **Status Summary**

- ? **BlazorWebApp**: Working with authentication  
- ? **MVC WebApp**: **NOW FIXED** with authentication + Customer terminology
- ? **WinForms App**: Working with direct database access
- ? **API**: Secure with global authentication requirement
- ? **IdentityServer**: Configured with all required clients

**The MVC WebApp is now fully functional!** ??