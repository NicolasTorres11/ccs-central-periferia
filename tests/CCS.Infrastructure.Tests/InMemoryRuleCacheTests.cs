using CCS.Domain.Entities;
using CCS.Domain.Enums;
using CCS.Domain.ValueObjects;
using CCS.Infrastructure.Caching;

namespace CCS.Infrastructure.Tests;

public class InMemoryRuleCacheTests
{
    [Fact]
    public async Task GetRulesAsync_WhenRulesWereConfigured_ReturnsRules()
    {
        var cache = new InMemoryRuleCache();
        var deviceId = new DeviceId("DEV-001");
        var rule = Rule.Create(Guid.NewGuid(), null, "Panico", EventType.Panic);

        cache.SetRules(deviceId, EventType.Panic, [rule]);

        var rules = await cache.GetRulesAsync(deviceId, EventType.Panic, CancellationToken.None);

        Assert.Single(rules);
        Assert.Equal(rule.RuleId, rules[0].RuleId);
    }
}

