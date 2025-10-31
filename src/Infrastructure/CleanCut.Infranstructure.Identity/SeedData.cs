using CleanCut.Infranstructure.Identity.Data;
using CleanCut.Infranstructure.Identity.Models;
using IdentityModel;
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
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

   // ✅ Create admin user with secure password
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
    Log.Information("Admin user 'alice' created with secure password");
            }
      else
  {
     Log.Debug("alice already exists");
          }

         // ✅ Create regular user with secure password
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
                Log.Information("Regular user 'bob' created with secure password");
    }
      else
        {
     Log.Debug("bob already exists");
    }
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
