using System.Text.Json.Serialization;
using OpsFlow.Api.Contracts;
using OpsFlow.Api.Domain;
using OpsFlow.Api.Services;
using Microsoft.EntityFrameworkCore;
using OpsFlow.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var saPassword = builder.Configuration["MSSQL_SA_PASSWORD"];
if (string.IsNullOrWhiteSpace(saPassword))
    throw new InvalidOperationException("MSSQL_SA_PASSWORD is required. Run: set -a; source .env; set +a");

var connStr =
    $"Server=localhost,1433;Database=OpsFlowDb;User Id=sa;Password={saPassword};TrustServerCertificate=True;Encrypt=False;";

builder.Services.AddDbContext<OpsFlowDbContext>(opt =>
    opt.UseSqlServer(connStr, sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped<IRequestService, EfRequestService>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/requests", (CreateRequestDto dto, IRequestService service) =>
{
    try
    {
        var created = service.Create(dto);
        return Results.Created($"/requests/{created.Id}", ToDto(created));
    }
    catch (ValidationException ex)
    {
        return Results.ValidationProblem(ex.Errors);
    }
});

app.MapGet("/requests", (string? status, string? q, IRequestService service) =>
{
    RequestStatus? parsedStatus = null;

    if (!string.IsNullOrWhiteSpace(status))
    {
        if (!Enum.TryParse<RequestStatus>(status, ignoreCase: true, out var s))
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Invalid status value." }
            });

        parsedStatus = s;
    }

    var list = service.List(parsedStatus, q).Select(ToDto);
    return Results.Ok(list);
});

app.MapGet("/requests/{id:int}", (int id, IRequestService service) =>
{
    var item = service.Get(id);
    return item is null
        ? Results.NotFound(new { message = $"Request {id} not found." })
        : Results.Ok(ToDto(item));
});

app.MapGet("/health/db", async (OpsFlowDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return Results.Ok(new { canConnect });
});


app.MapPatch("/requests/{id:int}", (int id, UpdateRequestDto dto, IRequestService service) =>
{
    var ok = service.TryUpdate(id, dto, out var updated, out var errors, out var notFoundMsg);

    if (!ok && notFoundMsg is not null)
        return Results.NotFound(new { message = notFoundMsg });

    if (!ok && errors is not null)
        return Results.ValidationProblem(errors);

    return Results.Ok(ToDto(updated!));
});

app.Run();

static RequestDto ToDto(RequestItem r) =>
    new(r.Id, r.Title, r.Description, r.Status, r.Priority, r.Notes, r.CreatedAt, r.UpdatedAt);
