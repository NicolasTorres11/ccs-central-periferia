using CCS.Application.Abstractions;
using CCS.Application.Emergency;
using CCS.Domain.Entities;
using CCS.Domain.Enums;
using CCS.Domain.ValueObjects;
using CCS.Infrastructure.Caching;
using CCS.Infrastructure.Messaging;
using CCS.Shared.Contracts;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<InMemoryRuleCache>();
builder.Services.AddSingleton<IRuleCache>(sp => sp.GetRequiredService<InMemoryRuleCache>());
builder.Services.AddSingleton<InMemoryActionPublisher>();
builder.Services.AddSingleton<IActionPublisher>(sp => sp.GetRequiredService<InMemoryActionPublisher>());
builder.Services.AddScoped<HandlePanicHandler>();

var app = builder.Build();

SeedEmergencyRules(app.Services);

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "CCS.Api.Emergency" }));

app.MapPost("/emergency", async (
    EmergencySignal signal,
    HttpContext http,
    HandlePanicHandler handler,
    CancellationToken cancellationToken) =>
{
    var correlationId = GetCorrelationId(http);
    var result = await handler.HandleAsync(new HandlePanicCommand(signal, correlationId), cancellationToken);

    return result.IsSuccess
        ? Results.Accepted($"/emergency/{correlationId}", new
        {
            correlationId,
            status = result.Status,
            receivedAt = DateTimeOffset.UtcNow
        })
        : Results.BadRequest(new
        {
            correlationId,
            title = "Solicitud invalida",
            detail = result.Error
        });
});

app.MapGet("/debug/dispatched-actions", (InMemoryActionPublisher publisher) =>
{
    return Results.Ok(publisher.Published);
});

app.Run();

static string GetCorrelationId(HttpContext http)
{
    if (http.Request.Headers.TryGetValue("x-correlation-id", out var value) && !string.IsNullOrWhiteSpace(value))
    {
        return value.ToString();
    }

    return Guid.NewGuid().ToString("D");
}

static void SeedEmergencyRules(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var cache = scope.ServiceProvider.GetRequiredService<InMemoryRuleCache>();
    var rule = Rule.Create(Guid.NewGuid(), null, "Panico - autoridad y propietario", EventType.Panic, priority: 1);
    rule.AddAction(RuleAction.Create(ActionType.AuthorityCall, "Authority", "policia-123", order: 1, isCritical: true));
    rule.AddAction(RuleAction.Create(ActionType.Sms, "Owner", "+573001111111", order: 2, isCritical: true));

    cache.SetRules(new DeviceId("DEV-SMOKE"), EventType.Panic, [rule]);
}
