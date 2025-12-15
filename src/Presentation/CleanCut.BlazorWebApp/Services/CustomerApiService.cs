using CleanCut.Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;

namespace CleanCut.BlazorWebApp.Services;

public interface ICustomerApiService
{
    Task<List<CustomerInfo>> GetAllCustomersAsync(CancellationToken cancellationToken = default);
    Task<CustomerInfo?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerInfo> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerInfo> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCustomerAsync(Guid id, CancellationToken cancellationToken = default);
}

public class CustomerApiService : ICustomerApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerApiService> _logger;

    public CustomerApiService(HttpClient httpClient, ILogger<CustomerApiService> logger)
    {
      _httpClient = httpClient;
        // Initialize logger
        _logger = logger;
    }

    public async Task<List<CustomerInfo>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
    {
  try
        {
   var url = "/api/customers";
       _logger.LogInformation("Making GET request to: {Url}", url);
    var response = await _httpClient.GetAsync(url, cancellationToken);
     response.EnsureSuccessStatusCode();
   var customers = await response.Content.ReadFromJsonAsync<List<CustomerInfo>>(cancellationToken: cancellationToken) ?? new();
       _logger.LogInformation("Fetched {Count} customers", customers.Count);
       return customers;
      }
        catch (OperationCanceledException)
   {
        _logger.LogWarning("GetAllCustomersAsync canceled");
throw;
   }
    catch (Exception ex)
  {
  _logger.LogError(ex, "Error fetching customers");
   throw;
   }
    }

public async Task<CustomerInfo?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
     {
 var url = $"/api/customers/{id}";
            _logger.LogInformation("Making GET request to: {Url}", url);
   var response = await _httpClient.GetAsync(url, cancellationToken);
     if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
    return null;

   response.EnsureSuccessStatusCode();
      return await response.Content.ReadFromJsonAsync<CustomerInfo>(cancellationToken: cancellationToken);
 }
  catch (OperationCanceledException)
      {
          _logger.LogWarning("GetCustomerByIdAsync canceled for {CustomerId}", id);
     throw;
      }
 catch (Exception ex)
        {
       _logger.LogError(ex, "Error fetching customer {CustomerId}", id);
         throw;
        }
    }

    public async Task<CustomerInfo> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
    try
   {
    var url = "/api/customers";
    _logger.LogInformation("Making POST request to: {Url} for customer {Email}", url, request.Email);

   var command = new
   {
      FirstName = request.FirstName,
  LastName = request.LastName,
      Email = request.Email
   };

  var response = await _httpClient.PostAsJsonAsync(url, command, cancellationToken);
 if (!response.IsSuccessStatusCode)
      {
  var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
    _logger.LogError("API returned error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
      }

       response.EnsureSuccessStatusCode();
      var customer = await response.Content.ReadFromJsonAsync<CustomerInfo>(cancellationToken: cancellationToken)
      ?? throw new InvalidOperationException("Failed to create customer");

    _logger.LogInformation("Successfully created customer {CustomerId}", customer.Id);
     return customer;
        }
  catch (OperationCanceledException)
        {
       _logger.LogWarning("CreateCustomerAsync canceled for {Email}", request.Email);
   throw;
      }
   catch (Exception ex)
  {
            _logger.LogError(ex, "Error creating customer {Email}", request.Email);
     throw;
        }
 }

    public async Task<CustomerInfo> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
      try
        {
    var url = $"/api/customers/{id}";
  _logger.LogInformation("Making PUT request to: {Url} for customer {Email}", url, request.Email);

       var command = new
          {
                Id = id,
      FirstName = request.FirstName,
    LastName = request.LastName,
 Email = request.Email
    };

            var response = await _httpClient.PutAsJsonAsync(url, command, cancellationToken);

    if (!response.IsSuccessStatusCode)
        {
      var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
      _logger.LogError("API returned error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
      }

  response.EnsureSuccessStatusCode();
   var customer = await response.Content.ReadFromJsonAsync<CustomerInfo>(cancellationToken: cancellationToken)
?? throw new InvalidOperationException("Failed to update customer");

  _logger.LogInformation("Successfully updated customer {CustomerId}", customer.Id);
      return customer;
  }
        catch (OperationCanceledException)
      {
            _logger.LogWarning("UpdateCustomerAsync canceled for {CustomerId}", id);
     throw;
 }
        catch (Exception ex)
   {
   _logger.LogError(ex, "Error updating customer {CustomerId}", id);
   throw;
        }
    }

    public async Task<bool> DeleteCustomerAsync(Guid id, CancellationToken cancellationToken = default)
    {
   try
        {
  var url = $"/api/customers/{id}";
            _logger.LogInformation("Making DELETE request to: {Url}", url);

          using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

          var response = await _httpClient.DeleteAsync(url, linked.Token);

     _logger.LogInformation("DELETE request completed. Status: {StatusCode}, Reason: {ReasonPhrase}",
   response.StatusCode, response.ReasonPhrase);

   if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
       _logger.LogWarning("Customer {CustomerId} not found during delete - may have already been deleted", id);
   return false; // Customer not found
   }

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
      {
   _logger.LogInformation("Customer {CustomerId} deleted successfully", id);
  return true; // Successfully deleted
    }

    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
          _logger.LogError("Delete failed for customer {CustomerId}. Status: {StatusCode}, Error: {ErrorContent}",
  id, response.StatusCode, errorContent);
        throw new HttpRequestException($"Delete failed with status {response.StatusCode}: {errorContent}");
     }

    return response.IsSuccessStatusCode;
   }
        catch (OperationCanceledException)
        {
    _logger.LogWarning("DeleteCustomerAsync canceled for {CustomerId}", id);
    throw;
        }
     catch (HttpRequestException httpEx)
        {
    _logger.LogError(httpEx, "HTTP error while deleting customer {CustomerId}. Message: {Message}", id, httpEx.Message);
    throw;
        }
        catch (Exception ex)
   {
       _logger.LogError(ex, "Unexpected error deleting customer {CustomerId}", id);
   throw;
  }
  }
}

// Local request models for API calls

public class CreateCustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateCustomerRequest
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}