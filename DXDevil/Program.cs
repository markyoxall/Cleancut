namespace DXDevil;

internal static class Program
{
    // Keep a DefaultLookAndFeel component alive for the app lifetime when
    // an instance-based look-and-feel is desired.
    private static DevExpress.LookAndFeel.DefaultLookAndFeel? s_defaultLookAndFeel;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        // Apply PerMonitorV2 DPI awareness previously stored in App.config DevExpress designer settings.
        try
        {
            Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.PerMonitorV2);
        }
        catch
        {
            // If running on platforms that don't support SetHighDpiMode, ignore and continue.
        }

        ApplicationConfiguration.Initialize();

        // Load connection string from appsettings.json if available
        string? defaultConn = null;
        try
        {
            var appSettingsPath = System.IO.Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (System.IO.File.Exists(appSettingsPath))
            {
                var json = System.IO.File.ReadAllText(appSettingsPath);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) && cs.TryGetProperty("CLEANCUT_DEFAULT_CONNECTION", out var val))
                {
                    defaultConn = val.GetString();
                }
            }
        }
        catch
        {
            // ignore and fallback to environment variable
        }

        // Enable form skinning so DevExpress can skin the non-client area
        // (caption/borders) where supported. If you reference the BonusSkins
        // assembly, register it from that assembly's API; avoid calling a
        // type that may not be present in all installations.
        DevExpress.Skins.SkinManager.EnableFormSkins();

        // Set the application-wide skin. You can change the skin quickly for testing
        // by setting the DX_SKIN environment variable (for example: "Office 2019 Colorful").
        var chosenSkin = System.Environment.GetEnvironmentVariable("DX_SKIN") ?? "The Bezier";

        // Create and keep a DefaultLookAndFeel component for the app lifetime.
        // This is the component you originally requested; it ensures the skin
        // is applied to DevExpress controls that respect the default look-and-feel.
        s_defaultLookAndFeel = new DevExpress.LookAndFeel.DefaultLookAndFeel();
        s_defaultLookAndFeel.LookAndFeel.SkinName = chosenSkin;
        System.Diagnostics.Debug.WriteLine($"DevExpress skin set to: {chosenSkin}");

        // Run the new main menu form which can open the other forms.
        Application.Run(new MainMenuForm(defaultConn));

        // no per-app component to dispose when using UserLookAndFeel.Default
    }
}
