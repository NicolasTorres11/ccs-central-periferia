using CCS.Domain.Enums;
using CCS.Functions.Dispatcher.Functions;
using CCS.Shared.Contracts;

namespace CCS.Functions.Dispatcher.Tests;

public sealed class ActionDispatcherFunctionTests
{
    [Fact]
    public void Handle_WhenActionHasMultipleTargets_ReturnsOrderedDispatchAttempts()
    {
        var function = new ActionDispatcherFunction();
        var dispatchedAction = new DispatchedAction(
            "correlation-1",
            "DEV-SMOKE",
            EventType.Panic,
            [
                new ActionRequest(ActionType.Sms, "Owner", "+573001111111", 2, true),
                new ActionRequest(ActionType.AuthorityCall, "Authority", "policia-123", 1, true)
            ],
            new { },
            IsCritical: true);

        var attempts = function.Handle(dispatchedAction);

        Assert.Collection(
            attempts,
            first => Assert.Equal("AuthorityCall", first.ActionType),
            second => Assert.Equal("Sms", second.ActionType));
    }

    [Fact]
    public void Handle_WhenThereAreNoActions_ReturnsEmptyAttempts()
    {
        var function = new ActionDispatcherFunction();
        var dispatchedAction = new DispatchedAction(
            "correlation-empty",
            "DEV-SMOKE",
            EventType.Panic,
            [],
            new { },
            IsCritical: true);

        var attempts = function.Handle(dispatchedAction);

        Assert.Empty(attempts);
    }

    [Fact]
    public void Handle_PreservesCriticalFlagAndTargetReference()
    {
        var function = new ActionDispatcherFunction();
        var dispatchedAction = new DispatchedAction(
            "correlation-2",
            "DEV-SMOKE",
            EventType.DriverDistress,
            [new ActionRequest(ActionType.WebhookOwner, "Custom", "https://example.test/hook", 1, false)],
            new { },
            IsCritical: true);

        var attempt = Assert.Single(function.Handle(dispatchedAction));

        Assert.Equal("correlation-2", attempt.CorrelationId);
        Assert.Equal("https://example.test/hook", attempt.TargetReference);
        Assert.False(attempt.IsCritical);
        Assert.Equal("Simulated", attempt.Status);
    }
}
