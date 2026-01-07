using OpsFlow.Api.Domain;

namespace OpsFlow.Api.Contracts;

public sealed record UpdateRequestDto(
    RequestStatus? Status,
    string? Notes
);
