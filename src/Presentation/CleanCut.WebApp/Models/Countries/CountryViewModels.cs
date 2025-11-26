using CleanCut.Application.DTOs;

namespace CleanCut.WebApp.Models.Countries;

public class CountryListViewModel
{
    public List<CountryInfo> Countries { get; set; } = new();
    public string? SearchTerm { get; set; }
    public bool? IsActiveFilter { get; set; }
    public int TotalCountries { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}

public class CountryEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public string CountryCode { get; set; }
}

public class CountryDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public string CountryCode { get; set; }
}