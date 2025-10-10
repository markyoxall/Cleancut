using CleanCut.Infrastructure.Data.Seeding;
using CleanCut.WinApp.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

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