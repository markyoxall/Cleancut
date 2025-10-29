namespace CleanCut.BlazorWebApp.Services.HttpClients;

public class CustomerApiOptions
{
  public string BaseUrl { get; set; } = "https://localhost:7142";
    public int TimeoutSeconds { get; set; } = 30;
}