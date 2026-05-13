namespace CCS.Shared.Contracts;

public sealed record TelemetryMessage(
    string DeviceId,
    DateTimeOffset Ts,
    GpsDto? Gps,
    double? SpeedKmh,
    double? TemperatureC,
    string EventType = "telemetry");

