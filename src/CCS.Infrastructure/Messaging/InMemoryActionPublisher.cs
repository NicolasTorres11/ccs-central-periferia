using CCS.Application.Abstractions;
using CCS.Shared.Contracts;

namespace CCS.Infrastructure.Messaging;

public sealed class InMemoryActionPublisher : IActionPublisher
{
    private readonly List<DispatchedAction> _published = [];

    public IReadOnlyList<DispatchedAction> Published => _published;

    public Task PublishCriticalAsync(DispatchedAction action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _published.Add(action);
        return Task.CompletedTask;
    }
}

