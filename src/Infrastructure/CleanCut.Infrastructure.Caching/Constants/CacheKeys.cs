namespace CleanCut.Infrastructure.Caching.Constants;

/// <summary>
/// Cache key constants and builders
/// </summary>
public static class CacheKeys
{
    // Cache key prefixes
    public const string CustomerPrefix = "customer";
    public const string ProductPrefix = "product";
    public const string CountryPrefix = "country";
    public const string OrderPrefix = "order";

    // Cache key builders
    public static string CustomerById(Guid id) => $"{CustomerPrefix}:id:{id}";
    public static string CustomerByEmail(string email) => $"{CustomerPrefix}:email:{email.ToLowerInvariant()}";
    public static string AllCustomers() => $"{CustomerPrefix}:all";

    public static string ProductById(Guid id) => $"{ProductPrefix}:id:{id}";
    public static string ProductsByCustomer(Guid customerId) => $"{ProductPrefix}:customer:{customerId}";
    public static string AllProducts() => $"{ProductPrefix}:all";

    public static string CountryById(Guid id) => $"{CountryPrefix}:id:{id}";
    public static string CountryByCode(string code) => $"{CountryPrefix}:code:{code.ToLowerInvariant()}";
    public static string AllCountries() => $"{CountryPrefix}:all";

    public static string OrderById(Guid id) => $"{OrderPrefix}:id:{id}";
    public static string AllOrders() => $"{OrderPrefix}:all";

    // Cache patterns for bulk removal
    public static string CustomerPattern() => $"{CustomerPrefix}:*";
    public static string ProductPattern() => $"{ProductPrefix}:*";
    public static string CountryPattern() => $"{CountryPrefix}:*";
    public static string OrderPattern() => $"{OrderPrefix}:*";
}
