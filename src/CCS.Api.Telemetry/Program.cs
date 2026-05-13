using CCS.Application.Telemetry;
using CCS.Shared.Contracts;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<ProcessTelemetryBatch>();
builder.Services.AddSingleton<InMemoryTelemetryBuffer>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "CCS.Api.Telemetry" }));

app.MapPost("/telemetry/ingest", (
    IReadOnlyList<TelemetryMessage> messages,
    ProcessTelemetryBatch useCase,
    InMemoryTelemetryBuffer buffer) =>
{
    var result = useCase.Validate(messages);
    if (!result.IsSuccess)
    {
        return Results.BadRequest(new { title = "Lote invalido", detail = result.Error });
    }

    buffer.Add(messages);
    return Results.Accepted("/telemetry/ingest", new
    {
        status = "accepted",
        count = messages.Count,
        receivedAt = DateTimeOffset.UtcNow
    });
});

app.MapGet("/debug/telemetry", (InMemoryTelemetryBuffer buffer) => Results.Ok(buffer.Messages));

app.Run();

internal sealed class InMemoryTelemetryBuffer
{
    private readonly List<TelemetryMessage> _messages = [];

    public IReadOnlyList<TelemetryMessage> Messages => _messages;

    public void Add(IEnumerable<TelemetryMessage> messages)
    {
        _messages.AddRange(messages);
    }
}

public partial class Program;
