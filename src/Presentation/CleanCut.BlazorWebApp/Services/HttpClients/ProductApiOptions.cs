namespace CleanCut.BlazorWebApp.Services.HttpClients;

public class ProductApiOptions
{
    public string BaseUrl { get; set; } = "https://localhost:7142";
    public int TimeoutSeconds { get; set; } = 30;
}