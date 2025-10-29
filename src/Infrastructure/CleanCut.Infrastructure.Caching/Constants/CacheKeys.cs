namespace CleanCut.Infrastructure.Caching.Constants;

/// <summary>
/// Cache key constants and builders
/// </summary>
public static class CacheKeys
{
    // Cache key prefixes
    public const string CustomerPrefix = "customer";
    public const string ProductPrefix = "product";
    
    // Cache key builders
    public static string CustomerById(Guid id) => $"{CustomerPrefix}:id:{id}";
    public static string CustomerByEmail(string email) => $"{CustomerPrefix}:email:{email.ToLowerInvariant()}";
    public static string AllCustomers() => $"{CustomerPrefix}:all";
    
    public static string ProductById(Guid id) => $"{ProductPrefix}:id:{id}";
    public static string ProductsByCustomer(Guid customerId) => $"{ProductPrefix}:customer:{customerId}";
    public static string AllProducts() => $"{ProductPrefix}:all";
    
    // Cache patterns for bulk removal
    public static string CustomerPattern() => $"{CustomerPrefix}:*";
    public static string ProductPattern() => $"{ProductPrefix}:*";
}