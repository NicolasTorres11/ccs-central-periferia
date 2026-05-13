using CCS.Application.Abstractions;
using CCS.Domain.Entities;
using CCS.Domain.Enums;
using CCS.Domain.ValueObjects;

namespace CCS.Infrastructure.Caching;

public sealed class InMemoryRuleCache : IRuleCache
{
    private readonly Dictionary<(string DeviceId, EventType EventType), List<Rule>> _rules = [];

    public void SetRules(DeviceId deviceId, EventType eventType, IEnumerable<Rule> rules)
    {
        _rules[(Normalize(deviceId), eventType)] = rules.ToList();
    }

    public Task<IReadOnlyList<Rule>> GetRulesAsync(DeviceId deviceId, EventType eventType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<Rule>>(
            _rules.TryGetValue((Normalize(deviceId), eventType), out var rules) ? rules : []);
    }

    private static string Normalize(DeviceId deviceId) => deviceId.Value.ToUpperInvariant();
}
