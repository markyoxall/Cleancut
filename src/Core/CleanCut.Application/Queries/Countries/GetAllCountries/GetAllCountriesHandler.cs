using AutoMapper;
using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanCut.Application.Queries.Countries.GetAllCountries;

internal class GetAllCountriesHandler : IRequestHandler<GetAllCountriesQuery, List<CountryInfo>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllCountriesHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CountryInfo>> Handle(GetAllCountriesQuery request, CancellationToken cancellationToken)
    {
        var countries = await _unitOfWork.Countries.GetAllAsync(cancellationToken);
        return _mapper.Map<List<CountryInfo>>(countries);
    }
}
