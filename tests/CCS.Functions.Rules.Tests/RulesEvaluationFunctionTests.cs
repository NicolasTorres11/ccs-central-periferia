using CCS.Domain.Enums;
using CCS.Functions.Rules.Functions;
using CCS.Shared.Contracts;

namespace CCS.Functions.Rules.Tests;

public sealed class RulesEvaluationFunctionTests
{
    [Fact]
    public void Handle_WhenTelemetryEventIsPanic_ReturnsEmergencySignal()
    {
        var function = new RulesEvaluationFunction();
        var message = new TelemetryMessage("DEV-SMOKE", DateTimeOffset.UtcNow, null, 0, null, "panic");

        var signal = function.Handle(message);

        Assert.NotNull(signal);
        Assert.Equal(EventType.Panic, signal.Type);
        Assert.Equal("DEV-SMOKE", signal.DeviceId);
    }

    [Fact]
    public void Handle_WhenTelemetryEventIsNotCritical_ReturnsNull()
    {
        var function = new RulesEvaluationFunction();
        var message = new TelemetryMessage("DEV-SMOKE", DateTimeOffset.UtcNow, null, 48, null);

        var signal = function.Handle(message);

        Assert.Null(signal);
    }

    [Theory]
    [InlineData("driver_distress", EventType.DriverDistress)]
    [InlineData("accident", EventType.Accident)]
    public void Handle_WhenTelemetryEventIsCritical_ReturnsExpectedEmergencyType(string telemetryEvent, EventType expectedType)
    {
        var function = new RulesEvaluationFunction();
        var message = new TelemetryMessage("DEV-SMOKE", DateTimeOffset.UtcNow, new GpsDto(4.65, -74.1), null, null, telemetryEvent);

        var signal = function.Handle(message);

        Assert.NotNull(signal);
        Assert.Equal(expectedType, signal.Type);
        Assert.Equal("RulesFunction", signal.Source);
        Assert.Contains(telemetryEvent, signal.Reason);
    }
}
