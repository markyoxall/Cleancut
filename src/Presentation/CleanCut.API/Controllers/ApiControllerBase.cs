using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCut.API.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IMediator Mediator { get; }

    protected ApiControllerBase(IMediator mediator)
    {
        Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    protected Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        => Mediator.Send(request, cancellationToken);
}
