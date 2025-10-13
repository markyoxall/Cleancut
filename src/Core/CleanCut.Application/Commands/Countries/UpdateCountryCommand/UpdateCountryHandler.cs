using AutoMapper;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Repositories;
using MediatR;

namespace CleanCut.Application.Commands.Countries.UpdateCountryCommand;

/// <summary>
/// Handler for updating an existing country
/// </summary>
internal class UpdateCountryHandler : IRequestHandler<UpdateCountryCommand, CountryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCountryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CountryDto> Handle(UpdateCountryCommand request, CancellationToken cancellationToken)
    {
        // Retrieve the country entity
        var country = await _unitOfWork.Countries.GetByIdAsync(request.Id, cancellationToken);
        if (country == null)
        {
            throw new InvalidOperationException($"Country with ID '{request.Id}' not found");
        }

        // Update properties
        country.UpdateDetails(request.Name, request.Code);

        // Explicitly update the entity in the repository
        await _unitOfWork.Countries.UpdateAsync(country, cancellationToken);


        // Persist changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO and return
        return _mapper.Map<CountryDto>(country);
    }
}