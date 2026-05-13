using CCS.Domain.Entities;
using CCS.Domain.Enums;
using CCS.Domain.ValueObjects;

namespace CCS.Domain.Tests;

public class RuleTests
{
    [Fact]
    public void Create_WhenPriorityIsOutsideRange_Throws()
    {
        var ownerId = Guid.NewGuid();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Rule.Create(ownerId, null, "Panico", EventType.Panic, priority: 9));
    }

    [Fact]
    public void Actions_ReturnsActionsOrderedByConfiguredOrder()
    {
        var rule = Rule.Create(Guid.NewGuid(), null, "Panico", EventType.Panic, priority: 1);
        rule.AddAction(RuleAction.Create(ActionType.Sms, "Owner", "+573001111111", order: 2, isCritical: true));
        rule.AddAction(RuleAction.Create(ActionType.AuthorityCall, "Authority", "policia-123", order: 1, isCritical: true));

        var actions = rule.Actions.ToArray();

        Assert.Equal(ActionType.AuthorityCall, actions[0].ActionType);
        Assert.Equal(ActionType.Sms, actions[1].ActionType);
    }

    [Fact]
    public void AppliesTo_WhenRuleIsInactive_ReturnsFalse()
    {
        var rule = Rule.Create(Guid.NewGuid(), null, "Panico", EventType.Panic);
        rule.Deactivate();

        Assert.False(rule.AppliesTo(new DeviceId("DEV-001"), EventType.Panic));
    }
}

