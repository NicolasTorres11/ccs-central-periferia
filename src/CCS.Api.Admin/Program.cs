using CCS.Domain.Enums;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<AdminStore>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "CCS.Api.Admin" }));

app.MapPost("/owners", (OwnerCreateRequest request, AdminStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.DocumentNumber) || string.IsNullOrWhiteSpace(request.FullName))
    {
        return Results.BadRequest(new { title = "Solicitud invalida", detail = "Documento y nombre son obligatorios." });
    }

    var owner = new OwnerDto(Guid.NewGuid(), request.DocumentType, request.DocumentNumber, request.FullName, request.Email, request.PhoneE164, "Active");
    store.Owners.Add(owner);
    return Results.Created($"/owners/{owner.OwnerId}", owner);
});

app.MapGet("/vehicles", (AdminStore store) => Results.Ok(new { items = store.Vehicles, nextCursor = (string?)null }));

app.MapPost("/vehicles", (VehicleCreateRequest request, AdminStore store) =>
{
    if (request.OwnerId == Guid.Empty || string.IsNullOrWhiteSpace(request.Plate) || string.IsNullOrWhiteSpace(request.DeviceId))
    {
        return Results.BadRequest(new { title = "Solicitud invalida", detail = "OwnerId, placa y deviceId son obligatorios." });
    }

    var vehicle = new VehicleDto(Guid.NewGuid(), request.OwnerId, request.VehicleType, request.Plate, request.DeviceId, "Active");
    store.Vehicles.Add(vehicle);
    return Results.Created($"/vehicles/{vehicle.VehicleId}", vehicle);
});

app.MapGet("/rules", (Guid? vehicleId, EventType? eventType, AdminStore store) =>
{
    var query = store.Rules.AsEnumerable();
    if (vehicleId.HasValue)
    {
        query = query.Where(rule => rule.VehicleId == vehicleId.Value);
    }

    if (eventType.HasValue)
    {
        query = query.Where(rule => rule.EventType == eventType.Value);
    }

    return Results.Ok(new { items = query.ToArray(), nextCursor = (string?)null });
});

app.MapPost("/rules", (RuleCreateRequest request, AdminStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Actions.Count == 0)
    {
        return Results.BadRequest(new { title = "Solicitud invalida", detail = "Nombre y al menos una accion son obligatorios." });
    }

    var rule = new RuleDto(
        Guid.NewGuid(),
        request.OwnerId,
        request.VehicleId,
        request.Name,
        request.EventType,
        request.Priority,
        request.IsActive,
        request.Actions);

    store.Rules.Add(rule);
    return Results.Created($"/rules/{rule.RuleId}", rule);
});

app.Run();

internal sealed class AdminStore
{
    public List<OwnerDto> Owners { get; } = [];

    public List<VehicleDto> Vehicles { get; } = [];

    public List<RuleDto> Rules { get; } = [];
}

internal sealed record OwnerCreateRequest(string DocumentType, string DocumentNumber, string FullName, string? Email, string? PhoneE164);

internal sealed record OwnerDto(Guid OwnerId, string DocumentType, string DocumentNumber, string FullName, string? Email, string? PhoneE164, string Status);

internal sealed record VehicleCreateRequest(Guid OwnerId, string VehicleType, string Plate, string DeviceId);

internal sealed record VehicleDto(Guid VehicleId, Guid OwnerId, string VehicleType, string Plate, string DeviceId, string Status);

internal sealed record RuleCreateRequest(
    Guid OwnerId,
    Guid? VehicleId,
    string Name,
    EventType EventType,
    int Priority,
    bool IsActive,
    IReadOnlyList<RuleActionDto> Actions);

internal sealed record RuleDto(
    Guid RuleId,
    Guid OwnerId,
    Guid? VehicleId,
    string Name,
    EventType EventType,
    int Priority,
    bool IsActive,
    IReadOnlyList<RuleActionDto> Actions);

internal sealed record RuleActionDto(ActionType ActionType, string TargetType, string TargetReference, int Order, bool IsCritical);
