using CleanCut.Application;
using CleanCut.Infrastructure.Data;
using CleanCut.Infrastructure.Caching;
using CleanCut.Infrastructure.Data.Seeding;
using CleanCut.API.Middleware;
using CleanCut.API.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add global exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ? Enhanced problem details for security
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Only apply Problem Details to API routes
        if (!context.HttpContext.Request.Path.StartsWithSegments("/api"))
   return;

    // ? Don't expose sensitive information in production
        if (!builder.Environment.IsDevelopment())
        {
  // Remove machine name and detailed errors in production
    context.ProblemDetails.Extensions.Remove("machine");
        }
        else
        {
      context.ProblemDetails.Extensions.TryAdd("machine", Environment.MachineName);
        }
      
 context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
     context.ProblemDetails.Extensions.TryAdd("timestamp", DateTime.UtcNow);
      context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;

        if (context.ProblemDetails.Type == null)
        {
       context.ProblemDetails.Type = context.ProblemDetails.Status switch
          {
400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
      401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
  500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
       _ => "https://tools.ietf.org/html/rfc7231"
  };
        }
    };
});

// ? Add rate limiting for security
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
         partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
  {
    AutoReplenishment = true,
                PermitLimit = builder.Environment.IsDevelopment() ? 1000 : 100, // More restrictive in production
                Window = TimeSpan.FromMinutes(1)
       }));
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
    };
});

// ? Secure CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
  {
        var allowedOrigins = builder.Environment.IsDevelopment()
   ? new[] {
              "https://localhost:7297", // Blazor HTTPS
       "http://localhost:5091"   // Blazor HTTP (dev only)
   }
            : builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

 policy.WithOrigins(allowedOrigins)
   .AllowAnyMethod()
      .AllowAnyHeader()
            .AllowCredentials()
    .SetPreflightMaxAge(TimeSpan.FromMinutes(5));
    });

    // ? Separate policy for Swagger (dev only)
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowSwagger", policy =>
        {
 policy.AllowAnyOrigin()
 .AllowAnyMethod()
                .AllowAnyHeader();
     });
 }
});

// ? Enhanced logging configuration
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.SetMinimumLevel(builder.Environment.IsDevelopment() ? LogLevel.Debug : LogLevel.Information);
});

// Get API configuration
var apiTitle = builder.Configuration["ApiSettings:Title"] ?? "CleanCut API";
var apiDescription = builder.Configuration["ApiSettings:Description"] ?? "CleanCut API";

// ? OpenAPI with enhanced security documentation
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
 document.Info = new OpenApiInfo
        {
    Title = apiTitle,
        Description = apiDescription,
            Version = "v1",
      Contact = new OpenApiContact
            {
        Name = "CleanCut API Support",
    Email = "support@cleancut.com"
            },
 // ? Add security information
            License = new OpenApiLicense
            {
 Name = "Proprietary",
      Url = new Uri("https://cleancut.com/license")
     }
        };

      // ? Enhanced JWT Bearer security scheme
        document.Components ??= new OpenApiComponents();
  document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
        {
      ["Bearer"] = new OpenApiSecurityScheme
     {
       Type = SecuritySchemeType.Http,
    Scheme = "bearer",
   BearerFormat = "JWT",
       Description = "Enter your JWT token in the format: Bearer {your token}",
             Name = "Authorization",
     In = ParameterLocation.Header
         }
        };

      var securityRequirement = new OpenApiSecurityRequirement
        {
            {
    new OpenApiSecurityScheme
        {
    Reference = new OpenApiReference
{
               Type = ReferenceType.SecurityScheme,
          Id = "Bearer"
        }
    },
    Array.Empty<string>()
          }
        };

        foreach (var pathItem in document.Paths.Values)
  {
            foreach (var operation in pathItem.Operations.Values)
            {
      operation.Security ??= new List<OpenApiSecurityRequirement>();
     operation.Security.Add(securityRequirement);
            }
   }

        return Task.CompletedTask;
    });
});

// Add Application layer
builder.Services.AddApplication();

// Add Data Infrastructure layer
builder.Services.AddDataInfrastructure(builder.Configuration);

// Add Caching Infrastructure layer
builder.Services.AddCachingInfrastructure(builder.Configuration);

// ? Enhanced JWT authentication configuration
var identityServerAuthority = builder.Configuration["IdentityServer:Authority"] ?? "https://localhost:5001";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = identityServerAuthority;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // ? Require HTTPS in production
    options.SaveToken = false; // ? Don't save tokens for security

    // ? Enhanced security events with debugging
  options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
   var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
   logger.LogWarning("JWT Authentication failed: {Message} for IP: {IpAddress}. Token: {Token}", 
      context.Exception.Message, 
    context.HttpContext.Connection.RemoteIpAddress,
           context.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "")?.Substring(0, Math.Min(50, context.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "")?.Length ?? 0)) + "...");
return Task.CompletedTask;
  },
        OnTokenValidated = context =>
 {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("JWT Token validated successfully for user: {User} from IP: {IpAddress}",
   context.Principal?.Identity?.Name ?? "Unknown",
context.HttpContext.Connection.RemoteIpAddress);
         
    // ? Log audience claim for debugging
     var audienceClaim = context.Principal?.FindFirst("aud")?.Value;
            logger.LogDebug("Token audience claim: {Audience}", audienceClaim);
        
          return Task.CompletedTask;
        },
    OnChallenge = context =>
        {
         var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Challenge triggered. Error: {Error}, Description: {Description} for IP: {IpAddress}",
     context.Error, context.ErrorDescription, context.HttpContext.Connection.RemoteIpAddress);
            return Task.CompletedTask;
        }
    };

    // ? Enhanced token validation parameters with better audience handling
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
  ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
   ValidIssuer = identityServerAuthority,
  // ? Accept multiple possible audience formats
 ValidAudiences = new[] { 
  "CleanCutAPI",            // API Resource name
            identityServerAuthority + "/resources",          // IdentityServer resources endpoint
      identityServerAuthority     // IdentityServer authority
        },
 ClockSkew = TimeSpan.FromMinutes(1), // ? Reduce clock skew tolerance
        RequireExpirationTime = true,
        RequireSignedTokens = true,
        // ? Custom audience validation for better debugging
 AudienceValidator = (audiences, token, validationParameters) =>
        {
            var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
     logger.LogDebug("Token audiences: {Audiences}", string.Join(", ", audiences));
            logger.LogDebug("Valid audiences: {ValidAudiences}", string.Join(", ", validationParameters.ValidAudiences));
   
            // Check if any of the token audiences match our valid audiences
    var isValid = audiences.Any(aud => validationParameters.ValidAudiences.Contains(aud));
     if (!isValid)
          {
    logger.LogWarning("Audience validation failed. Token audiences: {TokenAudiences}, Expected: {ExpectedAudiences}", 
              string.Join(", ", audiences), 
             string.Join(", ", validationParameters.ValidAudiences));
     }
            return isValid;
        }
    };
});

// ? Enhanced authorization with role-based access
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // ? Add role-based policies
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

// ? Add HSTS in production
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
  options.MaxAge = TimeSpan.FromDays(365);
    });
}

var app = builder.Build();

// ? Enhanced security middleware pipeline
if (app.Environment.IsDevelopment())
{
    // ? Add request logging middleware first (dev only)
    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Incoming request: {Method} {Path} from {Origin}",
       context.Request.Method,
            context.Request.Path,
            context.Request.Headers.Origin.FirstOrDefault() ?? "No Origin");

        await next();

        logger.LogInformation("Response: {StatusCode} for {Method} {Path}",
            context.Response.StatusCode,
context.Request.Method,
       context.Request.Path);
    });

    app.UseCors("AllowBlazorApp");
    app.UseStaticFiles();

    // ? Anonymous routes for development only
    app.MapGet("/", () => Results.Redirect("/token-helper.html")).AllowAnonymous();
    app.MapGet("/index.html", () => Results.Redirect("/versions.html")).AllowAnonymous();
    app.MapGet("/versions", () => Results.Redirect("/versions.html")).AllowAnonymous();
    app.MapGet("/token-helper", () => Results.Redirect("/token-helper.html")).AllowAnonymous();

    app.MapOpenApi().AllowAnonymous();

    app.UseSwaggerUI(options =>
 {
     options.SwaggerEndpoint(ApiConstants.OpenApiJson, ApiConstants.SwaggerUiTitle);
        options.RoutePrefix = ApiConstants.SwaggerUiRoutePrefix;
        options.DocumentTitle = ApiConstants.SwaggerUiTitle;
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
      options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.ShowExtensions();
 options.InjectStylesheet("/custom-swagger.css");
        options.EnablePersistAuthorization();
        options.OAuthClientId("swagger-ui");
        options.OAuthAppName("CleanCut API - Swagger UI");
        options.OAuthUsePkce();
    });

    // ? Seed database only in development
    using (var scope = app.Services.CreateScope())
    {
        await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
    }
}
else
{
    // ? Production security middleware
    app.UseHsts();
    app.UseCors("AllowBlazorApp");
}

// ? Security middleware pipeline order
app.UseExceptionHandler();
app.UseHttpsRedirection();

// ? Add rate limiting
app.UseRateLimiter();

// ? Add security headers middleware
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    
    headers.TryAdd("X-Content-Type-Options", "nosniff");
  headers.TryAdd("X-Frame-Options", "DENY");
    headers.TryAdd("X-XSS-Protection", "1; mode=block");
    headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    headers.TryAdd("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
  
    if (!app.Environment.IsDevelopment())
    {
        headers.TryAdd("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    }
    
    var csp = "default-src 'self'; " +
        "script-src 'self'; " +
      "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self'; " +
  "font-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'";
          
    if (!app.Environment.IsDevelopment())
    {
        csp += "; upgrade-insecure-requests";
    }
    
    headers.TryAdd("Content-Security-Policy", csp);
    
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
