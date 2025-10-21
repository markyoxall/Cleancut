using CleanCut.Application.DTOs;

namespace CleanCut.BlazorWebApp.Services;

public class V2ProductListResponse
{
    public List<ProductInfo> Data { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
    public string ApiVersion { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class V2ProductResponse
{
    public ProductInfo Data { get; set; } = new();
    public string ApiVersion { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class PaginationInfo
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

public class V2StatsResponse
{
    public StatsData Data { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class StatsData
{
    public int TotalProducts { get; set; }
    public int AvailableProducts { get; set; }
    public int UnavailableProducts { get; set; }
    public decimal AveragePrice { get; set; }
    public DateTime LastUpdated { get; set; }
}