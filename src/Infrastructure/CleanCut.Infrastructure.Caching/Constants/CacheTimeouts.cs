using System;

namespace CleanCut.Infrastructure.Caching.Constants;

public static class CacheTimeouts
{
    public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan Medium = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan Long = TimeSpan.FromHours(2);
    public static readonly TimeSpan VeryLong = TimeSpan.FromHours(24);

    public static readonly TimeSpan Products = Medium;
    public static readonly TimeSpan Customers = Long;
    public static readonly TimeSpan Countries = VeryLong;
    public static readonly TimeSpan Orders = TimeSpan.FromMinutes(15);
}
