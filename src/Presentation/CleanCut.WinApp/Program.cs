/*
 * CleanCut WinForms Application Program Entry Point
 * ================================================
 * 
 * This file configures the CleanCut WinForms desktop application which will serve as
 * a native client application in the OAuth2/OpenID Connect authentication architecture.
 * It demonstrates desktop app authentication patterns for enterprise scenarios.
 * 
 * ROLE IN AUTHENTICATION ARCHITECTURE (FUTURE IMPLEMENTATION):
 * -----------------------------------------------------------
 * This WinForms app will act as a PUBLIC CLIENT that:
 * 
 * 1. USER AUTHENTICATION - Uses system browser for IdentityServer login
 * 2. PKCE FLOW - Implements Authorization Code + PKCE for desktop security
 * 3. API INTEGRATION - Uses access tokens to call CleanCut.API
 * 4. LOCAL TOKEN STORAGE - Securely stores tokens using Windows Data Protection
 * 
 * PLANNED AUTHENTICATION FLOW:
 * ---------------------------
 * 
 * • Authorization Code + PKCE Flow:
 *   ??? User clicks "Login" ? Opens system browser to IdentityServer
 *   ??? User authenticates ? Browser redirected to localhost callback
 *   ??? App captures authorization code from callback URL
 *   ??? App exchanges code + PKCE verifier for tokens
 *   ??? Stores encrypted tokens locally for API access
 * 
 * • Token Management:
 *   ??? Access tokens cached with Windows Data Protection API
 *   ??? Automatic token refresh using refresh tokens
 *   ??? Secure token cleanup on application exit
 * 
 * DESKTOP-SPECIFIC AUTHENTICATION CHALLENGES:
 * ------------------------------------------
 * 
 * • Browser Integration:
 *   ??? Launch system browser for authentication (not embedded browser)
 *   ??? HTTP listener for localhost callback URL handling
 *   ??? Deep linking support for authentication responses
 * 
 * • Security Considerations:
 *   ??? Public client (no client secret) - relies on PKCE
 *   ??? Secure local token storage using Windows DPAPI
 *   ??? Protection against token theft and replay attacks
 *   ??? Automatic token cleanup on app uninstall
 * 
 * • User Experience:
 *   ??? Single sign-on with system browser
 *   ??? Remember authentication state across app restarts
 *   ??? Graceful handling of network connectivity issues
 *   ??? Clear authentication status indicators in UI
 * 
 * INTEGRATION WITH AUTHENTICATION ECOSYSTEM:
 * -----------------------------------------
 * 
 * • CleanCut.Infrastructure.Identity (IdentityServer):
 *   ??? Will configure WinApp as PKCE-enabled public client
 *   ??? Localhost redirect URIs for desktop callback handling
 *   ??? Appropriate token lifetimes for desktop scenarios
 * 
 * • CleanCut.API:
 *   ??? Will receive authenticated requests from desktop app
 *   ??? Same JWT validation as web clients
 *   ??? User-specific data based on token claims
 *   ??? Role-based access control for desktop users
 * 
 * • Comparison with Other Clients:
 *   ??? vs. BlazorWebApp: User auth vs. client credentials only
 *   ??? vs. WebApp (MVC): Similar user flow, different UI platform
 *   ??? vs. Mobile apps: Similar PKCE flow, different platform APIs
 * 
 * PLANNED AUTHENTICATION COMPONENTS:
 * ---------------------------------
 * 
 * • AuthenticationService (Future):
 *   ??? Handles OAuth2 flows using system browser
 *   ??? PKCE implementation for desktop security
 *   ??? Token storage using Windows Data Protection
 *   ??? Automatic token refresh and lifecycle management
 * 
 * • ApiService (Future):
 *   ??? HttpClient with automatic Bearer token injection
 *   ??? Similar to web app AuthenticatedHttpMessageHandler
 *   ??? API call abstraction for MVP presenters
 * 
 * • MVP Pattern Integration:
 *   ??? Presenters will use authenticated API services
 *   ??? Views will reflect user authentication state
 *   ??? Login/logout functionality in main form
 *   ??? Role-based UI element visibility
 * 
 * SECURITY ARCHITECTURE:
 * ---------------------
 * 
 * • Public Client Security:
 *   ??? No client secret (public clients can't keep secrets)
 *   ??? PKCE required for authorization code security
 *   ??? State parameter for CSRF protection
 *   ??? Nonce for replay attack prevention
 * 
 * • Local Storage Security:
 *   ??? Windows Data Protection API for token encryption
 *   ??? User-specific key derivation
 *   ??? Secure deletion of tokens on logout
 *   ??? Protection against local privilege escalation
 * 
 * • Network Security:
 *   ??? HTTPS-only communication with IdentityServer and API
 *   ??? Certificate pinning for production deployments
 *   ??? Timeout and retry policies for network calls
 * 
 * CURRENT IMPLEMENTATION STATUS:
 * -----------------------------
 * • MVP Pattern: ? Implemented for UI architecture
 * • Service Layer: ? Basic structure in place
 * • Authentication: ? Planned for future implementation
 * • API Integration: ? Depends on authentication implementation
 * 
 * AUTHENTICATION IMPLEMENTATION ROADMAP:
 * -------------------------------------
 * 1. Add OAuth2/OIDC client library (e.g., IdentityModel.OidcClient)
 * 2. Implement AuthenticationService with PKCE flow
 * 3. Create secure token storage using Windows DPAPI
 * 4. Add authentication UI components to main form
 * 5. Integrate authentication with existing MVP presenters
 * 6. Configure IdentityServer client for desktop app
 * 7. Test end-to-end authentication and API access
 * 
 * CONFIGURATION REQUIREMENTS (FUTURE):
 * -----------------------------------
 * • Client ID: "CleanCutWinApp"
 * • No Client Secret (public client)
 * • Redirect URIs: http://localhost:8080/, cleancut://callback
 * • Scopes: openid, profile, CleanCutAPI
 * • Response Type: code (Authorization Code)
 * • PKCE: Required
 * • Refresh Tokens: Enabled for offline access
 */

using CleanCut.Infrastructure.Data.Seeding;
using CleanCut.WinApp.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using CleanCut.WinApp.Services;

namespace CleanCut.WinApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main()
    {
        try
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // Configure services
            var serviceProvider = ServiceConfiguration.ConfigureServices();
            
            // Get logger
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("CleanCut.WinApp.Program");
            logger.LogInformation("Starting CleanCut WinApp application");
            
            // Seed database in development
            try
            {
                using var scope = serviceProvider.CreateScope();
                await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
                logger.LogInformation("Database seeding completed");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Database seeding failed, but application will continue");
            }
            
            // Create and run main form
            // Start SignalR notifications client if available
            try
            {
                var notifications = serviceProvider.GetService<INotificationsClient>();
                var mediator = serviceProvider.GetService<INotificationMediator>();
                if (notifications != null && mediator != null)
                {
                    // Wire client events to mediator
                    notifications.CustomerUpdated += async dto => await mediator.RaiseCustomerUpdated(dto);
                    notifications.ProductCreated += async dto => await mediator.RaiseProductCreated(dto);
                    notifications.ProductUpdated += async dto => await mediator.RaiseProductUpdated(dto);
                    notifications.CountryCreated += async dto => await mediator.RaiseCountryCreated(dto);
                    notifications.CountryUpdated += async dto => await mediator.RaiseCountryUpdated(dto);
                    notifications.OrderCreated += async dto => await mediator.RaiseOrderCreated(dto);
                    notifications.OrderUpdated += async dto => await mediator.RaiseOrderUpdated(dto);
                    notifications.OrderStatusChanged += async dto => await mediator.RaiseOrderStatusChanged(dto);

                    // Start the SignalR client in background and log errors
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await notifications.StartAsync();
                        }
                        catch (Exception ex)
                        {
                            var log = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("SignalRClient");
                            log?.LogWarning(ex, "SignalR client failed to start");
                        }
                    });
                }
            }
            catch { }

            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            System.Windows.Forms.Application.Run(mainForm);
            
            logger.LogInformation("CleanCut WinApp application stopped");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            MessageBox.Show($"A fatal error occurred: {ex.Message}", "Fatal Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
