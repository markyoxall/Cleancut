using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanCut.Infrastructure.Data.Entities;

[Table("IdempotencyRecords")]
public class IdempotencyRecord
{
    [Key]
    public string Key { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }

    public string? RequestHash { get; set; }

    public string? ResponsePayload { get; set; }

    public int? ResponseStatus { get; set; }

    public string? ResponseHeaders { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
