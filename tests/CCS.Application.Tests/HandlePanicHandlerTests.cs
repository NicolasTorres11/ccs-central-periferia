using CCS.Application.Abstractions;
using CCS.Application.Emergency;
using CCS.Domain.Entities;
using CCS.Domain.Enums;
using CCS.Domain.ValueObjects;
using CCS.Shared.Contracts;

namespace CCS.Application.Tests;

public class HandlePanicHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenPanicRulesExist_PublishesCriticalDispatch()
    {
        var cache = new FakeRuleCache();
        var publisher = new FakePublisher();
        var rule = Rule.Create(Guid.NewGuid(), null, "Panico a autoridad", EventType.Panic, priority: 1);
        rule.AddAction(RuleAction.Create(ActionType.AuthorityCall, "Authority", "policia-123", isCritical: true));
        cache.Rules = [rule];

        var handler = new HandlePanicHandler(cache, publisher);
        var signal = new EmergencySignal(
            "DEV-001",
            EventType.Panic,
            "PhysicalButton",
            new GpsDto(4.711, -74.072),
            DateTimeOffset.UtcNow);

        var result = await handler.HandleAsync(new HandlePanicCommand(signal, "corr-1"));

        Assert.True(result.IsSuccess);
        Assert.Equal("queued", result.Status);
        Assert.Single(publisher.Published);
        Assert.True(publisher.Published[0].IsCritical);
        Assert.Equal(ActionType.AuthorityCall, publisher.Published[0].Actions[0].ActionType);
    }

    [Fact]
    public async Task HandleAsync_WhenEventIsNotEmergency_ReturnsInvalid()
    {
        var handler = new HandlePanicHandler(new FakeRuleCache(), new FakePublisher());
        var signal = new EmergencySignal("DEV-001", EventType.OverSpeed, "AutoDetection", null, DateTimeOffset.UtcNow);

        var result = await handler.HandleAsync(new HandlePanicCommand(signal, "corr-2"));

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid", result.Status);
    }

    [Fact]
    public async Task HandleAsync_WhenCorrelationIdIsMissing_ReturnsInvalid()
    {
        var handler = new HandlePanicHandler(new FakeRuleCache(), new FakePublisher());
        var signal = new EmergencySignal("DEV-001", EventType.Panic, "PhysicalButton", null, DateTimeOffset.UtcNow);

        var result = await handler.HandleAsync(new HandlePanicCommand(signal, ""));

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid", result.Status);
    }

    [Fact]
    public async Task HandleAsync_WhenRulesDoNotExist_PublishesCriticalDispatchWithoutActions()
    {
        var publisher = new FakePublisher();
        var handler = new HandlePanicHandler(new FakeRuleCache(), publisher);
        var signal = new EmergencySignal("DEV-001", EventType.DriverDistress, "MobileApp", null, DateTimeOffset.UtcNow);

        var result = await handler.HandleAsync(new HandlePanicCommand(signal, "corr-empty"));

        Assert.True(result.IsSuccess);
        var dispatch = Assert.Single(publisher.Published);
        Assert.Empty(dispatch.Actions);
        Assert.Equal(EventType.DriverDistress, dispatch.EventType);
    }

    private sealed class FakeRuleCache : IRuleCache
    {
        public IReadOnlyList<Rule> Rules { get; set; } = [];

        public Task<IReadOnlyList<Rule>> GetRulesAsync(DeviceId deviceId, EventType eventType, CancellationToken cancellationToken)
        {
            return Task.FromResult(Rules);
        }
    }

    private sealed class FakePublisher : IActionPublisher
    {
        public List<DispatchedAction> Published { get; } = [];

        public Task PublishCriticalAsync(DispatchedAction action, CancellationToken cancellationToken)
        {
            Published.Add(action);
            return Task.CompletedTask;
        }
    }
}
