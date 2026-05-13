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

    [Fact]
    public async Task GetRulesAsync_WhenDeviceIdCasingDiffers_ReturnsRules()
    {
        var cache = new InMemoryRuleCache();
        var rule = Rule.Create(Guid.NewGuid(), null, "Panico", EventType.Panic);

        cache.SetRules(new DeviceId("dev-001"), EventType.Panic, [rule]);

        var rules = await cache.GetRulesAsync(new DeviceId("DEV-001"), EventType.Panic, CancellationToken.None);

        Assert.Single(rules);
    }

    [Fact]
    public async Task GetRulesAsync_WhenRulesDoNotExist_ReturnsEmptyCollection()
    {
        var cache = new InMemoryRuleCache();

        var rules = await cache.GetRulesAsync(new DeviceId("DEV-404"), EventType.Panic, CancellationToken.None);

        Assert.Empty(rules);
    }

    [Fact]
    public async Task GetRulesAsync_WhenCancellationIsRequested_Throws()
    {
        var cache = new InMemoryRuleCache();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            cache.GetRulesAsync(new DeviceId("DEV-001"), EventType.Panic, cts.Token));
    }
}
