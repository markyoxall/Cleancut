namespace CleanCut.Application.Common.Models;

public class IdempotencyEntry
{
    public string Key { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UserId { get; set; }
    public string? RequestHash { get; set; }
    public string? ResponsePayload { get; set; }
    public int? ResponseStatus { get; set; }
    public string? ResponseHeaders { get; set; }
}
