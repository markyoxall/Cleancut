using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace CleanCut.BlazorWebApp.Controllers;

[ApiController]
[Route("openapi")]
public class OpenApiProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenApiProxyController> _logger;

    public OpenApiProxyController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<OpenApiProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("proxy")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOpenApiProxy()
    {
        // Read API base url from configuration (same pattern used by API clients)
        var apiBase = _configuration["ApiClients:BaseUrl"] ?? "https://localhost:7142";

        try
        {
            var client = _httpClientFactory.CreateClient();

            // Attempt to fetch the OpenAPI document without authentication.
            // Some deployments expose the OpenAPI JSON publicly; if the API requires
            // authentication the response will be forwarded to the caller.
            var resp = await client.GetAsync(apiBase.TrimEnd('/') + "/openapi/v1.json");
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch OpenAPI from API: {Status}", resp.StatusCode);
                return StatusCode((int)resp.StatusCode);
            }

            var content = await resp.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying OpenAPI document");
            return StatusCode(500);
        }
    }
}
