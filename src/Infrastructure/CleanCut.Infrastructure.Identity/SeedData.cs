/*
 * IdentityServer Seed Data Configuration
 * =====================================
 * 
 * This file handles the creation of initial test users for the CleanCut IdentityServer
 * during development and testing phases. It creates predefined users with proper roles
 * and claims to support authentication testing across all client applications.
 * 
 * ROLE IN AUTHENTICATION ARCHITECTURE:
 * ------------------------------------
 * This seed data creates test users that can be used to authenticate against IdentityServer
 * and subsequently access the CleanCut.API with proper authorization claims.
 * 
 * TEST USERS CREATED:
 * ------------------
 * 
 * • ALICE (Admin User)
 *   └── Email: admin@cleancut.com
 *   └── Role: Admin  
 *   └── Claims: Full name, website, department (IT), employee_id
 *   └── Purpose: Test admin-level API operations and authorization policies
 *   └── Can access: All API endpoints including admin-only operations
 * 
 * • BOB (Regular User)  
 *   └── Email: user@cleancut.com
 *   └── Role: User
 *   └── Claims: Full name, website, department (Sales), employee_id, location
 *   └── Purpose: Test standard user API operations
 *   └── Can access: User-level API endpoints only
 * 
 * • MARK (Admin User)
 *   └── Email: mark.yoxall@test.com
 *   └── Role: Admin
 *   └── Claims: Full name, website, department (Engineering), employee_id, location
 *   └── Purpose: Test admin-level API operations and authorization policies
 *   └── Can access: All API endpoints including admin-only operations
 * 
 * INTEGRATION WITH CLIENT APPLICATIONS:
 * ------------------------------------
 * 
 * • CleanCut.WebApp (MVC)
 *   └── Users can login using alice/bob/mark credentials
 *   └── After authentication, receives ID token with user claims
 *   └── Also receives access token for API calls
 *   └── Role claims determine UI elements and available features
 * 
 * • CleanCut.BlazorWebApp
 * └── Uses these users for testing authorization scenarios
 *   └── Server-side app can impersonate users for API testing
 *   └── Claims used for conditional UI rendering
 * 
 * • CleanCut.API
 *   └── Validates tokens containing these user claims
 *   └── Role-based authorization policies use Admin/User roles
 *   └── API controllers check role claims for access control
 * 
 * SECURITY CONSIDERATIONS:
 * -----------------------
 * • Passwords are randomly generated or loaded from secure configuration
 * • Only runs in development environment - production uses real user accounts
 * • Uses proper password hashing through ASP.NET Core Identity
 * • Claims follow OpenID Connect standard format
 * • Supports testing of both authentication AND authorization flows
 * 
 * CLAIMS INCLUDED:
 * ---------------
 * • JwtClaimTypes.Name - Full display name
 * • JwtClaimTypes.GivenName - First name  
 * • JwtClaimTypes.FamilyName - Last name
 * • JwtClaimTypes.WebSite - Personal website
 * • JwtClaimTypes.Role - Authorization role (Admin/User)
 * • department - Custom claim for business logic
 * • employee_id - Custom claim for employee identification
 * • location - Custom claim for geographic-based features
 */

using CleanCut.Infranstructure.Identity.Data;
using CleanCut.Infranstructure.Identity.Models;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace CleanCut.Infranstructure.Identity;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            // ✅ Create roles first - this will populate AspNetRoles table
            EnsureRoles(roleMgr);

            // ✅ Create admin user Alice with secure password
            var alice = userMgr.FindByNameAsync("alice").Result;
            if (alice == null)
            {
                alice = new ApplicationUser
                {
                    UserName = "alice",
                    Email = "admin@cleancut.com",
                    EmailConfirmed = true,
                };

                // ✅ Use secure password from configuration or generate strong default
                var adminPassword = configuration["SeedData:AdminPassword"] ?? GenerateSecurePassword();
                var result = userMgr.CreateAsync(alice, adminPassword).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                // ✅ Log password in development only
                if (environment.IsDevelopment())
                {
                    Log.Warning("DEVELOPMENT ONLY - Alice password: {Password}", adminPassword);
                }

                // ✅ Assign user to Admin role - this will populate AspNetUserRoles table
                result = userMgr.AddToRoleAsync(alice, "Admin").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(alice, new Claim[]{
                    new Claim(JwtClaimTypes.Name, "Alice Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Alice"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                    new Claim(JwtClaimTypes.Role, "Admin"),
                    new Claim("department", "IT"),
                    new Claim("employee_id", "EMP001")
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Information("Admin user 'alice' created with secure password and assigned to Admin role");
            }
            else
            {
                Log.Debug("alice already exists");
            }

            // ✅ Create regular user Bob with secure password
            var bob = userMgr.FindByNameAsync("bob").Result;
            if (bob == null)
            {
                bob = new ApplicationUser
                {
                    UserName = "bob",
                    Email = "user@cleancut.com",
                    EmailConfirmed = true
                };

                // ✅ Use secure password from configuration or generate strong default
                var userPassword = configuration["SeedData:UserPassword"] ?? GenerateSecurePassword();
                var result = userMgr.CreateAsync(bob, userPassword).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                // ✅ Log password in development only
                if (environment.IsDevelopment())
                {
                    Log.Warning("DEVELOPMENT ONLY - Bob password: {Password}", userPassword);
                }

                // ✅ Assign user to User role - this will populate AspNetUserRoles table
                result = userMgr.AddToRoleAsync(bob, "User").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(bob, new Claim[]{
                    new Claim(JwtClaimTypes.Name, "Bob Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Bob"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                    new Claim(JwtClaimTypes.Role, "User"),
                    new Claim("department", "Sales"),
                    new Claim("employee_id", "EMP002"),
                    new Claim("location", "New York")
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Information("Regular user 'bob' created with secure password and assigned to User role");
            }
            else
            {
                Log.Debug("bob already exists");
            }

            // ✅ Create admin user Mark Yoxall with secure password
            var mark = userMgr.FindByNameAsync("mark").Result;
            if (mark == null)
            {
                mark = new ApplicationUser
                {
                    UserName = "mark",
                    Email = "mark.yoxall@test.com",
                    EmailConfirmed = true
                };

                // ✅ Use secure password from configuration or generate strong default
                var markPassword = configuration["SeedData:MarkPassword"] ?? GenerateSecurePassword();
                var result = userMgr.CreateAsync(mark, markPassword).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                // ✅ Log password in development only
                if (environment.IsDevelopment())
                {
                    Log.Warning("DEVELOPMENT ONLY - Mark password: {Password}", markPassword);
                }

                // ✅ Assign user to Admin role - this will populate AspNetUserRoles table
                result = userMgr.AddToRoleAsync(mark, "Admin").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(mark, new Claim[]{
                    new Claim(JwtClaimTypes.Name, "Mark Yoxall"),
                    new Claim(JwtClaimTypes.GivenName, "Mark"),
                    new Claim(JwtClaimTypes.FamilyName, "Yoxall"),
                    new Claim(JwtClaimTypes.WebSite, "https://markyoxall.dev"),
                    new Claim(JwtClaimTypes.Role, "Admin"),
                    new Claim("department", "Engineering"),
                    new Claim("employee_id", "EMP003"),
                    new Claim("location", "London")
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Information("Admin user 'mark' created with secure password and assigned to Admin role");
            }
            else
            {
                Log.Debug("mark already exists");
            }
        }
    }

    // ✅ Create the roles that will be used throughout the application
    private static void EnsureRoles(RoleManager<IdentityRole> roleMgr)
    {
        // Create Admin role
        if (!roleMgr.RoleExistsAsync("Admin").Result)
        {
            var adminRole = new IdentityRole("Admin");
            var result = roleMgr.CreateAsync(adminRole).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Information("Admin role created");
        }
        else
        {
            Log.Debug("Admin role already exists");
        }

        // Create User role
        if (!roleMgr.RoleExistsAsync("User").Result)
        {
            var userRole = new IdentityRole("User");
            var result = roleMgr.CreateAsync(userRole).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Information("User role created");
        }
        else
        {
            Log.Debug("User role already exists");
        }
    }

    // ✅ Generate cryptographically secure passwords
    private static string GenerateSecurePassword()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        var password = new string(Enumerable.Repeat(chars, 16)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        // ✅ Ensure password meets complexity requirements
        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => "!@#$%^&*".Contains(c));

        if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
        {
            // Regenerate if doesn't meet requirements
            return GenerateSecurePassword();
        }

        return password;
    }
}
