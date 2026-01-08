using CleanCut.Domain.Common;

namespace CleanCut.Domain.ValueObjects;

/// <summary>
/// Value object representing a person's name
/// </summary>
public readonly record struct PersonName
{
    public string FirstName { get; }
    public string LastName { get; }
    public string FullName => $"{FirstName} {LastName}".Trim();

    private PersonName(string firstName, string lastName)
    {
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Creates a PersonName with validation
    /// </summary>
    public static Result<PersonName> Create(string? firstName, string? lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result<PersonName>.Failure("First name cannot be empty");

        if (string.IsNullOrWhiteSpace(lastName))
            return Result<PersonName>.Failure("Last name cannot be empty");

        var trimmedFirstName = firstName.Trim();
        var trimmedLastName = lastName.Trim();

        if (trimmedFirstName.Length > 50)
            return Result<PersonName>.Failure("First name cannot exceed 50 characters");

        if (trimmedLastName.Length > 50)
            return Result<PersonName>.Failure("Last name cannot exceed 50 characters");

        return Result<PersonName>.Success(new PersonName(trimmedFirstName, trimmedLastName));
    }

    /// <summary>
    /// Tries to parse first and last names into a PersonName
    /// </summary>
    public static bool TryParse(string? firstName, string? lastName, out PersonName? personName)
    {
        var result = Create(firstName, lastName);
        personName = result.IsSuccess ? result.Value : null;
                return result.IsSuccess;
            }

            public override string ToString() => FullName;
        }
