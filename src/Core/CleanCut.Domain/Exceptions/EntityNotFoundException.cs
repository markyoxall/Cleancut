namespace CleanCut.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityName, object entityId) 
        : base($"{entityName} with identifier '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}