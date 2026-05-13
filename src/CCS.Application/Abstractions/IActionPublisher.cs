using CCS.Shared.Contracts;

namespace CCS.Application.Abstractions;

public interface IActionPublisher
{
    Task PublishCriticalAsync(DispatchedAction action, CancellationToken cancellationToken);
}

