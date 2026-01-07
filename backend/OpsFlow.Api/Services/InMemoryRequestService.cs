using System.Collections.Concurrent;
using OpsFlow.Api.Contracts;
using OpsFlow.Api.Domain;

namespace OpsFlow.Api.Services;

public sealed class InMemoryRequestService : IRequestService
{
    private readonly ConcurrentDictionary<int, RequestItem> _db = new();
    private int _id = 0;

    public RequestItem Create(CreateRequestDto dto)
    {
        var errors = ValidateCreate(dto);
        if (errors.Count > 0) throw new ValidationException(errors);

        var id = Interlocked.Increment(ref _id);

        var item = new RequestItem
        {
            Id = id,
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Priority = dto.Priority,
            Status = RequestStatus.New,
            Notes = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db[id] = item;
        return item;
    }

    public RequestItem? Get(int id) => _db.TryGetValue(id, out var v) ? v : null;

    public IReadOnlyList<RequestItem> List(RequestStatus? status, string? q)
    {
        var query = _db.Values.AsEnumerable();

        if (status is not null)
            query = query.Where(r => r.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var needle = q.Trim().ToLowerInvariant();
            query = query.Where(r =>
                r.Title.ToLowerInvariant().Contains(needle) ||
                (r.Description?.ToLowerInvariant().Contains(needle) ?? false));
        }

        return query
            .OrderByDescending(r => r.UpdatedAt)
            .ToList();
    }

    public bool TryUpdate(
        int id,
        UpdateRequestDto dto,
        out RequestItem? updated,
        out Dictionary<string, string[]>? validationErrors,
        out string? notFoundMessage)
    {
        updated = null;
        validationErrors = null;
        notFoundMessage = null;

        if (!_db.TryGetValue(id, out var item))
        {
            notFoundMessage = $"Request {id} not found.";
            return false;
        }

        var errors = ValidateUpdate(dto, item);
        if (errors.Count > 0)
        {
            validationErrors = errors;
            return false;
        }

        if (dto.Status is not null)
            item.Status = dto.Status.Value;

        if (dto.Notes is not null)
            item.Notes = dto.Notes.Trim();

        item.UpdatedAt = DateTimeOffset.UtcNow;

        _db[id] = item;
        updated = item;
        return true;
    }

    private static Dictionary<string, string[]> ValidateCreate(CreateRequestDto dto)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(dto.Title))
            errors["title"] = new[] { "Title is required." };
        else if (dto.Title.Trim().Length > 120)
            errors["title"] = new[] { "Title must be <= 120 characters." };

        if (dto.Description is not null && dto.Description.Trim().Length > 2000)
            errors["description"] = new[] { "Description must be <= 2000 characters." };

        return errors;
    }

    private static Dictionary<string, string[]> ValidateUpdate(UpdateRequestDto dto, RequestItem current)
    {
        var errors = new Dictionary<string, string[]>();

        var hasStatus = dto.Status is not null;
        var hasNotes = dto.Notes is not null;

        if (!hasStatus && !hasNotes)
            errors["body"] = new[] { "Provide at least one of: status, notes." };

        if (dto.Notes is not null && dto.Notes.Trim().Length > 2000)
            errors["notes"] = new[] { "Notes must be <= 2000 characters." };

        if (dto.Status is not null && !IsValidTransition(current.Status, dto.Status.Value))
            errors["status"] = new[] { $"Invalid status transition: {current.Status} -> {dto.Status.Value}." };

        return errors;
    }

    private static bool IsValidTransition(RequestStatus from, RequestStatus to)
    {
        if (from == to) return true;

        return from switch
        {
            RequestStatus.New => to is RequestStatus.InProgress or RequestStatus.Cancelled,
            RequestStatus.InProgress => to is RequestStatus.Done or RequestStatus.Cancelled,
            RequestStatus.Done => false,
            RequestStatus.Cancelled => false,
            _ => false
        };
    }
}

public sealed class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors) : base("Validation failed.")
    {
        Errors = errors;
    }
}
