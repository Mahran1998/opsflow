namespace OpsFlow.Api.Domain;

public sealed class RequestItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.New;
    public RequestPriority Priority { get; set; } = RequestPriority.Normal;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
