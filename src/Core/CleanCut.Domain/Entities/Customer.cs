using CleanCut.Domain.Common;
using CleanCut.Domain.Events;
using CleanCut.Domain.ValueObjects;

namespace CleanCut.Domain.Entities;

/// <summary>
/// Represents a customer in the system (renamed from User)
/// </summary>
public class Customer : BaseEntity
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;

    public PersonName? Name { get; private set; }
    public EmailAddress? Email { get; private set; }
    public bool IsActive { get; private set; } = true;

    // EF Core backing fields
    public string FirstName
    {
        get => _firstName;
        private set => _firstName = value;
    }

    public string LastName
    {
        get => _lastName;
        private set => _lastName = value;
    }

    private string EmailValue
    {
        get => _email;
        set => _email = value;
    }

    // Private constructor for EF Core
    private Customer() 
    {
        // EF Core will populate backing fields, then we reconstruct value objects
    }

    // Public property accessor that reconstructs value objects from backing fields
    private void EnsureValueObjects()
    {
        if (Name == null && !string.IsNullOrEmpty(_firstName) && !string.IsNullOrEmpty(_lastName))
        {
            var nameResult = PersonName.Create(_firstName, _lastName);
            if (nameResult.IsSuccess)
                Name = nameResult.Value;
        }

        if (Email == null && !string.IsNullOrEmpty(_email))
        {
            var emailResult = EmailAddress.Create(_email);
            if (emailResult.IsSuccess)
                Email = emailResult.Value;
        }
    }

    private Customer(PersonName name, EmailAddress email)
    {
        Name = name;
        Email = email;
        _firstName = name.FirstName;
        _lastName = name.LastName;
        _email = email.Value;

        // Raise domain event
        AddDomainEvent(new CustomerCreatedEvent(this));
    }

    /// <summary>
    /// Factory method to create a Customer with validation
    /// </summary>
    public static Result<Customer> Create(string firstName, string lastName, string email)
    {
        var nameResult = PersonName.Create(firstName, lastName);
        if (!nameResult.IsSuccess)
            return Result<Customer>.Failure(nameResult.Error);

        var emailResult = EmailAddress.Create(email);
        if (!emailResult.IsSuccess)
            return Result<Customer>.Failure(emailResult.Error);

        return Result<Customer>.Success(new Customer(nameResult.Value!, emailResult.Value!));
    }

    public string GetFullName() => Name?.FullName ?? $"{FirstName} {LastName}".Trim();

    public Result UpdateName(string firstName, string lastName)
    {
        var nameResult = PersonName.Create(firstName, lastName);
        if (!nameResult.IsSuccess)
            return Result.Failure(nameResult.Error);

        Name = nameResult.Value;
        _firstName = nameResult.Value.FirstName;
        _lastName = nameResult.Value.LastName;
        SetUpdatedAt();

        // Raise domain event
        AddDomainEvent(new CustomerUpdatedEvent(this, "Name"));

        return Result.Success();
    }

    public Result UpdateEmail(string email)
    {
        var emailResult = EmailAddress.Create(email);
        if (!emailResult.IsSuccess)
            return Result.Failure(emailResult.Error);

        Email = emailResult.Value;
        _email = emailResult.Value.Value;
        SetUpdatedAt();

        // Raise domain event
        AddDomainEvent(new CustomerUpdatedEvent(this, "Email"));

        return Result.Success();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new CustomerUpdatedEvent(this, "Status"));
    }

        public void Activate()
        {
            IsActive = true;
            SetUpdatedAt();

            // Raise domain event
            AddDomainEvent(new CustomerUpdatedEvent(this, "Status"));
        }
    }
