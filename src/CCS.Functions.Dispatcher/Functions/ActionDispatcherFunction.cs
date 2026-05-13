using CCS.Shared.Contracts;

namespace CCS.Functions.Dispatcher.Functions;

public sealed class ActionDispatcherFunction
{
    public IReadOnlyList<DispatchAttempt> Handle(DispatchedAction dispatchedAction)
    {
        return dispatchedAction.Actions
            .OrderBy(action => action.Order)
            .Select(action => new DispatchAttempt(
                dispatchedAction.CorrelationId,
                action.ActionType.ToString(),
                action.TargetType,
                action.TargetReference,
                action.IsCritical,
                Status: "Simulated"))
            .ToArray();
    }
}

public sealed record DispatchAttempt(
    string CorrelationId,
    string ActionType,
    string TargetType,
    string TargetReference,
    bool IsCritical,
    string Status);
