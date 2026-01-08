using CleanCut.Domain.Common;

namespace CleanCut.Domain.ValueObjects;

/// <summary>
/// Value object representing an email address
/// </summary>
public readonly record struct EmailAddress
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value;
    }

    /// <summary>
    /// Creates an EmailAddress with validation
    /// </summary>
    public static Result<EmailAddress> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<EmailAddress>.Failure("Email cannot be empty");

        var trimmedEmail = value.Trim();

        if (!IsValid(trimmedEmail))
            return Result<EmailAddress>.Failure($"'{trimmedEmail}' is not a valid email address");

        if (trimmedEmail.Length > 255)
            return Result<EmailAddress>.Failure("Email cannot exceed 255 characters");

        return Result<EmailAddress>.Success(new EmailAddress(trimmedEmail.ToLowerInvariant()));
    }

    /// <summary>
    /// Tries to parse a string into an EmailAddress
    /// </summary>
    public static bool TryParse(string? value, out EmailAddress? emailAddress)
    {
        var result = Create(value);
        emailAddress = result.IsSuccess ? result.Value : null;
        return result.IsSuccess;
    }

        private static bool IsValid(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static implicit operator string(EmailAddress email) => email.Value ?? string.Empty;

        public override string ToString() => Value ?? string.Empty;
    }
