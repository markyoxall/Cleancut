namespace CleanCut.Domain.Common;

/// <summary>
/// Base class for domain events - pure domain concept
/// </summary>
public abstract class DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}