using CCS.Domain.Enums;

namespace CCS.Shared.Contracts;

public sealed record DispatchedAction(
    string CorrelationId,
    string DeviceId,
    EventType EventType,
    IReadOnlyList<ActionRequest> Actions,
    object Context,
    bool IsCritical);

public sealed record ActionRequest(
    ActionType ActionType,
    string TargetType,
    string TargetReference,
    int Order,
    bool IsCritical);

