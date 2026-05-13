using CCS.Domain.Enums;
using CCS.Infrastructure.Messaging;
using CCS.Shared.Contracts;

namespace CCS.Infrastructure.Tests;

public class InMemoryActionPublisherTests
{
    [Fact]
    public async Task PublishCriticalAsync_StoresPublishedAction()
    {
        var publisher = new InMemoryActionPublisher();
        var action = new DispatchedAction(
            "corr-1",
            "DEV-001",
            EventType.Panic,
            [new ActionRequest(ActionType.Sms, "Owner", "+573001111111", 1, true)],
            new { },
            true);

        await publisher.PublishCriticalAsync(action, CancellationToken.None);

        Assert.Single(publisher.Published);
        Assert.Equal("corr-1", publisher.Published[0].CorrelationId);
    }

    [Fact]
    public async Task PublishCriticalAsync_WhenCancellationIsRequested_ThrowsAndDoesNotStoreAction()
    {
        var publisher = new InMemoryActionPublisher();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var action = new DispatchedAction(
            "corr-1",
            "DEV-001",
            EventType.Panic,
            [],
            new { },
            true);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            publisher.PublishCriticalAsync(action, cts.Token));

        Assert.Empty(publisher.Published);
    }
}
