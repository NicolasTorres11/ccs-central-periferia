using CCS.Domain.Enums;

namespace CCS.Domain.Entities;

public sealed record RuleAction(
    Guid RuleActionId,
    ActionType ActionType,
    string TargetType,
    string TargetReference,
    int Order,
    bool IsCritical)
{
    public static RuleAction Create(ActionType actionType, string targetType, string targetReference, int order = 1, bool isCritical = false)
    {
        if (string.IsNullOrWhiteSpace(targetType))
        {
            throw new ArgumentException("El tipo de destino es obligatorio.", nameof(targetType));
        }

        if (string.IsNullOrWhiteSpace(targetReference))
        {
            throw new ArgumentException("La referencia de destino es obligatoria.", nameof(targetReference));
        }

        if (order < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser mayor o igual a 1.");
        }

        return new RuleAction(Guid.NewGuid(), actionType, targetType.Trim(), targetReference.Trim(), order, isCritical);
    }
}

