using MediatR;
using Microsoft.AspNetCore.Mvc;
using CleanCut.Application.DTOs;
using CleanCut.Application.Queries.Countries.GetAllCountries;
using CleanCut.Application.Queries.Countries.GetCountry;
using CleanCut.Application.Commands.Countries.CreateCountry;
using CleanCut.Application.Commands.Countries.UpdateCountryCommand;
using CleanCut.Application.Commands.Countries.DeleteCountry;

[ApiController]
[Route("api/[controller]")]
public class CountriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CountriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CountryDto>>> GetAll()
        => await _mediator.Send(new GetAllCountriesQuery());

    [HttpGet("{id}")]
    public async Task<ActionResult<CountryDto?>> Get(Guid id)
        => await _mediator.Send(new GetCountryQuery(id));

    [HttpPost]
    public async Task<ActionResult<CountryDto>> Create(CreateCountryCommand command)
        => await _mediator.Send(command);

    [HttpPut("{id}")]
    public async Task<ActionResult<CountryDto>> Update(Guid id, UpdateCountryCommand command)
    {
        if (id != command.Id) return BadRequest();
        return await _mediator.Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(Guid id)
        => await _mediator.Send(new DeleteCountryCommand(id));
}