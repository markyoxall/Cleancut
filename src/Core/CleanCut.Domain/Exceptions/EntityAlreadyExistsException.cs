namespace CleanCut.Domain.Exceptions;

/// <summary>
/// Exception thrown when an entity already exists
/// </summary>
public class EntityAlreadyExistsException : DomainException
{
    public string EntityName { get; }
    public string PropertyName { get; }
    public object PropertyValue { get; }

    public EntityAlreadyExistsException(string entityName, string propertyName, object propertyValue) 
        : base($"{entityName} with {propertyName} '{propertyValue}' already exists.")
    {
        EntityName = entityName;
        PropertyName = propertyName;
        PropertyValue = propertyValue;
    }
}