using AutoMapper;
using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace CleanCut.Application.Queries.Countries.GetCountry;

internal class GetCountryHandler : IRequestHandler<GetCountryQuery, CountryDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCountryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CountryDto?> Handle(GetCountryQuery request, CancellationToken cancellationToken)
    {
        var country = await _unitOfWork.Countries.GetByIdAsync(request.Id, cancellationToken);
        return country == null ? null : _mapper.Map<CountryDto>(country);
    }
}