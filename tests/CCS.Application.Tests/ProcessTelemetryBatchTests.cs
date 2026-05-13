using CCS.Application.Telemetry;
using CCS.Shared.Contracts;

namespace CCS.Application.Tests;

public class ProcessTelemetryBatchTests
{
    [Fact]
    public void Validate_WhenBatchIsEmpty_ReturnsInvalid()
    {
        var useCase = new ProcessTelemetryBatch();

        var result = useCase.Validate([]);

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid", result.Status);
    }

    [Fact]
    public void Validate_WhenBatchHasValidTelemetry_ReturnsSuccess()
    {
        var useCase = new ProcessTelemetryBatch();
        var messages = new[]
        {
            new TelemetryMessage("DEV-001", DateTimeOffset.UtcNow, new GpsDto(4.711, -74.072), 48, 6.5)
        };

        var result = useCase.Validate(messages);

        Assert.True(result.IsSuccess);
        Assert.Equal("valid", result.Status);
    }

    [Fact]
    public void Validate_WhenBatchExceedsLimit_ReturnsInvalid()
    {
        var useCase = new ProcessTelemetryBatch();
        var messages = Enumerable
            .Range(1, 101)
            .Select(index => new TelemetryMessage($"DEV-{index:000}", DateTimeOffset.UtcNow, null, null, null))
            .ToArray();

        var result = useCase.Validate(messages);

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid", result.Status);
    }
}
