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

[ApiController]
[Route("api/[controller]")]
public class CountriesController : ApiControllerBase
{
   

    public CountriesController(IMediator mediator) : base(mediator)
    {
        
    }

    [HttpGet]
    public async Task<ActionResult<List<CountryInfo>>> GetAll()
        => await  Send(new GetAllCountriesQuery());

    [HttpGet("{id}")]
    public async Task<ActionResult<CountryInfo?>> Get(Guid id)
        => await  Send(new GetCountryQuery(id));

    [HttpPost]
    public async Task<ActionResult<CountryInfo>> Create(CreateCountryCommand command)
        => await  Send(command);

    [HttpPut("{id}")]
    public async Task<ActionResult<CountryInfo>> Update(Guid id, UpdateCountryCommand command)
    {
        if (id != command.Id) return BadRequest();
        return await  Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(Guid id)
        => await  Send(new DeleteCountryCommand(id));
}