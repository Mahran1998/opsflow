using OpsFlow.Api.Domain;

namespace OpsFlow.Api.Contracts;

public sealed record RequestDto(
    int Id,
    string Title,
    string? Description,
    RequestStatus Status,
    RequestPriority Priority,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
