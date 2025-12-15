# API Service Diagnostics

## Current Status Analysis

### ? Working Services:
- **Countries**: Gets data successfully

### ? Not Working Services:
- **Customers**: No data retrieved
- **Products**: No data on some pages  
- **Dashboard**: Shows products but missing customer count

## Possible Causes & Solutions

### 1. **Authentication Issues**
Since Countries works but Customers/Products don't, this might indicate:
- IdentityServer not running on port 5001
- Different authentication requirements for different endpoints
- Token scope issues

**Quick Test:**
```bash
# Test if IdentityServer is running
curl -k https://localhost:5001/.well-known/openid_configuration

# Test token acquisition
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=CleanCutBlazorWebApp&client_secret=BlazorServerSecret2024!&scope=CleanCutAPI"
```

### 2. **API Endpoint Differences**
Check if the API endpoints have different security requirements:
- Customers API might require different scopes
- Products API might have different authentication settings

### 3. **Service Configuration Issues**
Even though Countries works, there might be subtle differences in:
- HttpClient configuration
- BaseAddress settings
- Authentication handler attachment

## Debugging Steps

### Step 1: Check Console Logs
Look for:
- "Unauthorized request - check token validity"
- "JWT Authentication failed"
- HTTP 401/403 errors

### Step 2: Test API Endpoints Directly
```bash
# Test customers endpoint
curl https://localhost:7142/api/customers

# Test products endpoint
curl https://localhost:7142/api/v1/products

# Test countries endpoint (this one works)
curl https://localhost:7142/api/countries
```

### Step 3: Check Network Tab
In browser dev tools, look for:
- Failed HTTP requests
- 401/403 responses
- Missing Authorization headers

## Required Services Status

Make sure these are all running:
1. **IdentityServer**: `https://localhost:5001` ? Must be running first
2. **API**: `https://localhost:7142` ? 
3. **BlazorWebApp**: `https://localhost:7297` ?

## Next Steps

1. **Start IdentityServer first**: `dotnet run` in `src/Infrastructure/CleanCut.Infranstructure.Identity`
2. **Check logs** in both IdentityServer and API for authentication errors
3. **Test token acquisition** using the curl command above
4. **Check browser console** for JavaScript errors or failed HTTP requests

If Countries is working but Customers/Products aren't, the most likely cause is **authentication scope issues** or **IdentityServer not running**.