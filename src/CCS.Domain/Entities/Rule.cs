using CCS.Domain.Enums;
using CCS.Domain.ValueObjects;

namespace CCS.Domain.Entities;

public sealed class Rule
{
    private readonly List<RuleAction> _actions = [];

    private Rule(Guid ruleId, Guid ownerId, Guid? vehicleId, string name, EventType eventType, int priority, bool isActive)
    {
        RuleId = ruleId;
        OwnerId = ownerId;
        VehicleId = vehicleId;
        Name = name;
        EventType = eventType;
        Priority = priority;
        IsActive = isActive;
    }

    public Guid RuleId { get; }

    public Guid OwnerId { get; }

    public Guid? VehicleId { get; }

    public string Name { get; }

    public EventType EventType { get; }

    public int Priority { get; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<RuleAction> Actions => _actions.OrderBy(a => a.Order).ToArray();

    public static Rule Create(Guid ownerId, Guid? vehicleId, string name, EventType eventType, int priority = 3, bool isActive = true)
    {
        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("El propietario es obligatorio.", nameof(ownerId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre de la regla es obligatorio.", nameof(name));
        }

        if (priority is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(priority), "La prioridad debe estar entre 1 y 5.");
        }

        return new Rule(Guid.NewGuid(), ownerId, vehicleId, name.Trim(), eventType, priority, isActive);
    }

    public void AddAction(RuleAction action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _actions.Add(action);
    }

    public bool AppliesTo(DeviceId deviceId, EventType eventType)
    {
        _ = deviceId;
        return IsActive && EventType == eventType;
    }

    public void Deactivate() => IsActive = false;
}

