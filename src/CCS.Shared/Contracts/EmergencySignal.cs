using CCS.Domain.Enums;

namespace CCS.Shared.Contracts;

public sealed record EmergencySignal(
    string DeviceId,
    EventType Type,
    string Source,
    GpsDto? Gps,
    DateTimeOffset Timestamp,
    string? Reason = null);

