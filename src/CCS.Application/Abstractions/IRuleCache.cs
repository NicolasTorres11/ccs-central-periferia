using CCS.Domain.Entities;
using CCS.Domain.Enums;
using CCS.Domain.ValueObjects;

namespace CCS.Application.Abstractions;

public interface IRuleCache
{
    Task<IReadOnlyList<Rule>> GetRulesAsync(DeviceId deviceId, EventType eventType, CancellationToken cancellationToken);
}

