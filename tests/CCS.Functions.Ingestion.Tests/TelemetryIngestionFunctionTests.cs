using CCS.Application.Telemetry;
using CCS.Functions.Ingestion.Functions;
using CCS.Shared.Contracts;

namespace CCS.Functions.Ingestion.Tests;

public sealed class TelemetryIngestionFunctionTests
{
    [Fact]
    public void Handle_WhenTelemetryBatchIsValid_ReturnsSuccess()
    {
        var function = new TelemetryIngestionFunction(new ProcessTelemetryBatch());
        var messages = new[]
        {
            new TelemetryMessage("DEV-SMOKE", DateTimeOffset.UtcNow, new GpsDto(4.65, -74.1), 45, 6.5)
        };

        var result = function.Handle(messages);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Handle_WhenTelemetryBatchIsEmpty_ReturnsFailure()
    {
        var function = new TelemetryIngestionFunction(new ProcessTelemetryBatch());

        var result = function.Handle(Array.Empty<TelemetryMessage>());

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Handle_WhenTelemetryBatchExceedsLimit_ReturnsFailure()
    {
        var function = new TelemetryIngestionFunction(new ProcessTelemetryBatch());
        var messages = Enumerable
            .Range(1, 101)
            .Select(index => new TelemetryMessage($"DEV-{index:000}", DateTimeOffset.UtcNow, null, null, null))
            .ToArray();

        var result = function.Handle(messages);

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid", result.Status);
    }
}
