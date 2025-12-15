using System.Threading;
using System.Threading.Tasks;

namespace CleanCut.API.Services;

public interface IIntegrationEventProcessor
{
    Task ProcessAsync(string routingKey, object payload, CancellationToken cancellationToken = default);
}
