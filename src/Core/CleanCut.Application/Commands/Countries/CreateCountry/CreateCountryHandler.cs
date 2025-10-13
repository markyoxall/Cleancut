using AutoMapper;
using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Repositories;

namespace CleanCut.Application.Commands.Countries.CreateCountry;

/// <summary>
/// Handler for creating a new country
/// </summary>
internal class CreateCountryHandler : IRequestHandler<CreateCountryCommand, CountryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCountryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CountryDto> Handle(CreateCountryCommand request, CancellationToken cancellationToken)
    {
        // Create new country (use constructor if available)
        var country = new Country(request.Name, request.Code);

        // Add to repository
        await _unitOfWork.Countries.AddAsync(country, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return mapped DTO
        return _mapper.Map<CountryDto>(country);
    }
}