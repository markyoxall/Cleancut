using CleanCut.API.Controllers;
using CleanCut.Application.Commands.Countries.CreateCountry;
using CleanCut.Application.Commands.Countries.DeleteCountry;
using CleanCut.Application.Commands.Countries.UpdateCountryCommand;
using CleanCut.Application.DTOs;
using CleanCut.Application.Queries.Countries.GetAllCountries;
using CleanCut.Application.Queries.Countries.GetCountry;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// API Controller for Country operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // ✅ CRITICAL FIX: Require authentication for all endpoints
public class CountriesController : ApiControllerBase
{
    public CountriesController(IMediator mediator) : base(mediator)
    {
        
    }

    /// <summary>
    /// Get all countries - Requires authentication
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CountryInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<CountryInfo>>> GetAll()
        => await  Send(new GetAllCountriesQuery());

    /// <summary>
    /// Get country by ID - Requires authentication
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CountryInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CountryInfo?>> Get(Guid id)
        => await  Send(new GetCountryQuery(id));

    /// <summary>
    /// Create a new country - Requires authentication
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CountryInfo), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CountryInfo>> Create(CreateCountryCommand command)
        => await  Send(command);

    /// <summary>
    /// Update an existing country - Requires authentication
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CountryInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CountryInfo>> Update(Guid id, UpdateCountryCommand command)
    {
        if (id != command.Id) return BadRequest();
        return await  Send(command);
    }

    /// <summary>
    /// Delete a country - Requires Admin role for destructive operations
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")] // ✅ Enterprise security: Admin-only for destructive operations
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Delete(Guid id)
        => await  Send(new DeleteCountryCommand(id));
}