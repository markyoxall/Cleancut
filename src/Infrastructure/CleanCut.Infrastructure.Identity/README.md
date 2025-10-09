# CleanCut.Infrastructure.Identity - Identity & Authentication Layer

## Purpose in Clean Architecture

The **Infrastructure.Identity Layer** handles all **authentication, authorization, and user management** concerns. It provides secure user identity services while keeping these cross-cutting concerns separate from your core business logic. This layer implements security-related interfaces defined in the Application Layer.

## Key Principles

### 1. **Security First**
- Implements industry-standard security practices
- Handles password hashing, token generation, and validation
- Protects against common security vulnerabilities

### 2. **Framework Integration**
- Integrates with ASP.NET Core Identity or external identity providers
- Supports multiple authentication schemes (JWT, cookies, OAuth)
- Provides seamless identity management

### 3. **User Management**
- Handles user registration, login, and profile management
- Manages roles and permissions
- Supports user claims and external login providers

### 4. **Token-Based Authentication**
- JWT token generation and validation
- Refresh token management
- Secure token storage and transmission

## Folder Structure

```
CleanCut.Infrastructure.Identity/
??? Services/            # Identity service implementations
?   ??? AuthenticationService.cs
?   ??? AuthorizationService.cs
?   ??? UserService.cs
?   ??? TokenService.cs
?   ??? RoleService.cs
??? Models/              # Identity-specific models
?   ??? ApplicationUser.cs
?   ??? ApplicationRole.cs
?   ??? LoginModel.cs
?   ??? RegisterModel.cs
?   ??? TokenResponse.cs
??? Configuration/       # Identity configuration
?   ??? IdentityConfig.cs
?   ??? JwtConfig.cs
?   ??? SecuritySettings.cs
??? Extensions/          # Extension methods
?   ??? IdentityExtensions.cs
?   ??? ClaimsPrincipalExtensions.cs
??? Validators/          # Identity validation
?   ??? LoginValidator.cs
?   ??? RegisterValidator.cs
??? Context/            # Identity DbContext
    ??? IdentityDbContext.cs
```

## What Goes Here

### Authentication Services
- User login/logout functionality
- Password validation and reset
- External provider authentication (Google, Facebook, etc.)

### Authorization Services
- Role-based access control
- Permission checking
- Claims-based authorization

### Token Services
- JWT token generation
- Token validation and refresh
- Secure token management

### User Management
- User registration and profile management
- Password policies and enforcement
- Account lockout and security features

## Example Patterns

### Authentication Service
```csharp
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationService> _logger;

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return AuthenticationResult.Failure("Invalid credentials");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            await HandleFailedLogin(user);
            return AuthenticationResult.Failure("Invalid credentials");
        }

        var token = await _tokenService.GenerateTokenAsync(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return AuthenticationResult.Success(token, refreshToken);
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return AuthenticationResult.Failure("User already exists");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return AuthenticationResult.Failure(result.Errors.Select(e => e.Description));
        }

        await _userManager.AddToRoleAsync(user, "User");
        
        var token = await _tokenService.GenerateTokenAsync(user);
        
        return AuthenticationResult.Success(token);
    }
}
```

### Token Service
```csharp
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<ApplicationUser> _userManager;

    public async Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        // Add role claims
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Add custom claims
        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(ApplicationUser user)
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        
        var refreshToken = Convert.ToBase64String(randomBytes);
        
        // Store refresh token in database
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);
        
        await _userManager.UpdateAsync(user);
        
        return refreshToken;
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
```

### Authorization Service
```csharp
public class AuthorizationService : IAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var userClaims = await _userManager.GetClaimsAsync(user);
        if (userClaims.Any(c => c.Type == "permission" && c.Value == permission))
            return true;

        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                if (roleClaims.Any(c => c.Type == "permission" && c.Value == permission))
                    return true;
            }
        }

        return false;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Enumerable.Empty<string>();

        var permissions = new List<string>();

        // Direct user permissions
        var userClaims = await _userManager.GetClaimsAsync(user);
        permissions.AddRange(userClaims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value));

        // Role-based permissions
        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                permissions.AddRange(roleClaims
                    .Where(c => c.Type == "permission")
                    .Select(c => c.Value));
            }
        }

        return permissions.Distinct();
    }
}
```

## Key Technologies & Packages

### Required NuGet Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.Google" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.Facebook" Version="8.0.0" />
```

### Project References
- **CleanCut.Application** (implements authentication/authorization interfaces)

## Configuration Models

### JWT Settings
```csharp
public class JwtSettings
{
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationInMinutes { get; set; }
    public int RefreshTokenExpirationInDays { get; set; }
}

// In appsettings.json
{
  "JwtSettings": {
    "Secret": "YourVeryLongAndSecureSecretKeyHere",
    "Issuer": "CleanCut",
    "Audience": "CleanCutUsers",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

### Identity Configuration
```csharp
public static class IdentityConfig
{
    public static void ConfigureIdentity(this IServiceCollection services)
    {
        services.Configure<IdentityOptions>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
        });
    }
}
```

## Security Features

### Password Policies
- Minimum length requirements
- Character complexity requirements
- Password history to prevent reuse
- Secure password hashing (bcrypt/PBKDF2)

### Account Security
- Account lockout after failed attempts
- Two-factor authentication support
- Email confirmation
- Password reset functionality

### Token Security
- Short-lived access tokens
- Secure refresh token implementation
- Token blacklisting capability
- HTTPS-only token transmission

## Integration Patterns

### Dependency Injection Setup
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureIdentity();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
```

## Common Security Patterns

### Role-Based Authorization
```csharp
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    // Admin-only endpoints
}
```

### Permission-Based Authorization
```csharp
[Authorize(Policy = "CanManageUsers")]
public async Task<IActionResult> DeleteUser(string userId)
{
    // Implementation
}

// Policy configuration
services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireClaim("permission", "users.manage"));
});
```

### Claims-Based Authorization
```csharp
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IAuthorizationService _authorizationService;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        var hasPermission = await _authorizationService.HasPermissionAsync(userId, requirement.Permission);
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
```

## Testing Strategy

### Unit Tests
- Test authentication logic with mocked dependencies
- Verify token generation and validation
- Test authorization rules and policies

### Integration Tests
- Test complete authentication flows
- Verify database interactions with Identity stores
- Test external provider integrations

## Common Mistakes to Avoid

? **Storing Passwords in Plain Text** - Always hash passwords
? **Weak JWT Secrets** - Use strong, random secrets
? **Long-Lived Tokens** - Keep access tokens short-lived
? **Missing Token Validation** - Always validate tokens properly
? **Insecure Token Storage** - Use secure storage mechanisms

? **Strong Password Policies** - Enforce complexity requirements
? **Secure Token Management** - Implement proper token lifecycle
? **Role-Based Access Control** - Use granular permissions
? **Regular Security Audits** - Monitor and audit security practices
? **HTTPS Everywhere** - Never transmit tokens over HTTP

This layer is your security foundation - treat it with the highest priority and follow security best practices!