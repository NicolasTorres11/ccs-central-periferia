using CCS.Application.Abstractions;
using CCS.Application.Common;
using CCS.Domain.Enums;
using CCS.Domain.ValueObjects;
using CCS.Shared.Contracts;

namespace CCS.Application.Emergency;

public sealed class HandlePanicHandler(IRuleCache ruleCache, IActionPublisher actionPublisher)
{
    public async Task<Result> HandleAsync(HandlePanicCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.CorrelationId))
        {
            return Result.Failure("CorrelationId es obligatorio.", "invalid");
        }

        if (command.Signal.Type is not (EventType.Panic or EventType.DriverDistress))
        {
            return Result.Failure("El endpoint de emergencia solo acepta eventos criticos.", "invalid");
        }

        var deviceId = new DeviceId(command.Signal.DeviceId);
        var rules = await ruleCache.GetRulesAsync(deviceId, command.Signal.Type, cancellationToken);
        var actionRequests = rules
            .Where(rule => rule.AppliesTo(deviceId, command.Signal.Type))
            .SelectMany(rule => rule.Actions)
            .OrderBy(action => action.Order)
            .Select(action => new ActionRequest(
                action.ActionType,
                action.TargetType,
                action.TargetReference,
                action.Order,
                action.IsCritical))
            .ToArray();

        var dispatch = new DispatchedAction(
            command.CorrelationId,
            deviceId.Value,
            command.Signal.Type,
            actionRequests,
            command.Signal,
            IsCritical: true);

        await actionPublisher.PublishCriticalAsync(dispatch, cancellationToken);
        return Result.Success("queued");
    }
}

