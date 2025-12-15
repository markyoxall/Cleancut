namespace CleanCut.Domain.Exceptions;

/// <summary>
/// Exception thrown when business rule validation fails
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleValidationException(string message) 
        : base(message)
    {
        RuleName = "BusinessRule";
    }

    public BusinessRuleValidationException(string ruleName, string message) 
        : base(message)
    {
        RuleName = ruleName;
    }
}