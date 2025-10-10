namespace CleanCut.Infrastructure.Caching.Constants;

/// <summary>
/// Cache key constants and builders
/// </summary>
public static class CacheKeys
{
    // Cache key prefixes
    public const string UserPrefix = "user";
    public const string ProductPrefix = "product";
    
    // Cache key builders
    public static string UserById(Guid id) => $"{UserPrefix}:id:{id}";
    public static string UserByEmail(string email) => $"{UserPrefix}:email:{email.ToLowerInvariant()}";
    public static string AllUsers() => $"{UserPrefix}:all";
    
    public static string ProductById(Guid id) => $"{ProductPrefix}:id:{id}";
    public static string ProductsByUser(Guid userId) => $"{ProductPrefix}:user:{userId}";
    public static string AllProducts() => $"{ProductPrefix}:all";
    
    // Cache patterns for bulk removal
    public static string UserPattern() => $"{UserPrefix}:*";
    public static string ProductPattern() => $"{ProductPrefix}:*";
}