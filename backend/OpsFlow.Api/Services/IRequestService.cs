using OpsFlow.Api.Contracts;
using OpsFlow.Api.Domain;

namespace OpsFlow.Api.Services;

public interface IRequestService
{
    RequestItem Create(CreateRequestDto dto);
    RequestItem? Get(int id);
    IReadOnlyList<RequestItem> List(RequestStatus? status, string? q);
    bool TryUpdate(
        int id,
        UpdateRequestDto dto,
        out RequestItem? updated,
        out Dictionary<string, string[]>? validationErrors,
        out string? notFoundMessage);
}
