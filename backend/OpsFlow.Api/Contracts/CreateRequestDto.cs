using OpsFlow.Api.Domain;

namespace OpsFlow.Api.Contracts;

public sealed record CreateRequestDto(
    string Title,
    string? Description,
    RequestPriority Priority
);
