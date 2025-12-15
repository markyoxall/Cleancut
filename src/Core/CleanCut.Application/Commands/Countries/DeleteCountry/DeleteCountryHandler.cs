using CleanCut.Domain.Repositories;
using MediatR;

namespace CleanCut.Application.Commands.Countries.DeleteCountry;

/// <summary>
/// Handler for deleting a country
/// </summary>
internal class DeleteCountryHandler : IRequestHandler<DeleteCountryCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCountryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCountryCommand request, CancellationToken cancellationToken)
    {
        // Retrieve the country entity
        var country = await _unitOfWork.Countries.GetByIdAsync(request.Id, cancellationToken);
        if (country == null)
        {
            throw new InvalidOperationException($"Country with ID '{request.Id}' not found");
        }

        // Delete the entity
        await _unitOfWork.Countries.DeleteAsync(country, cancellationToken);

        // Persist changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}