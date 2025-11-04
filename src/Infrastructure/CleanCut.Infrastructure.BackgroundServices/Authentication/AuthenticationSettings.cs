namespace CleanCut.Infrastructure.BackgroundServices.Authentication;

/// <summary>
/// Authentication settings for background services connecting to IdentityServer
/// </summary>
public class AuthenticationSettings
{
    /// <summary>
    /// IdentityServer authority URL
    /// </summary>
    public string Authority { get; set; } = "https://localhost:5001";

    /// <summary>
    /// Client ID for authentication
    /// </summary>
    public string ClientId { get; set; } = "cleancut-background-service";

    /// <summary>
    /// Client secret for authentication
    /// </summary>
    public string ClientSecret { get; set; } = "BackgroundServiceSecret2024!";

    /// <summary>
    /// Scope to request
    /// </summary>
 public string Scope { get; set; } = "CleanCutAPI";

 /// <summary>
    /// Token endpoint override (optional)
    /// </summary>
    public string? TokenEndpoint { get; set; }
}