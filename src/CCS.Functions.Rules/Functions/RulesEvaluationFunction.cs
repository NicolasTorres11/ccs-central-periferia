using CCS.Domain.Enums;
using CCS.Shared.Contracts;

namespace CCS.Functions.Rules.Functions;

public sealed class RulesEvaluationFunction
{
    private static readonly HashSet<string> CriticalTelemetryEvents = new(StringComparer.OrdinalIgnoreCase)
    {
        "panic",
        "driver_distress",
        "accident"
    };

    public EmergencySignal? Handle(TelemetryMessage message)
    {
        if (!CriticalTelemetryEvents.Contains(message.EventType))
        {
            return null;
        }

        var eventType = message.EventType.Equals("driver_distress", StringComparison.OrdinalIgnoreCase)
            ? EventType.DriverDistress
            : message.EventType.Equals("accident", StringComparison.OrdinalIgnoreCase)
                ? EventType.Accident
                : EventType.Panic;

        return new EmergencySignal(
            message.DeviceId,
            eventType,
            Source: "RulesFunction",
            message.Gps,
            message.Ts,
            Reason: $"Evento critico derivado de telemetria: {message.EventType}");
    }
}
