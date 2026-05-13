using System.Diagnostics;
using CCS.Application.Telemetry;
using CCS.Shared.Contracts;

namespace CCS.LoadTests;

public sealed class TelemetryThroughputTests
{
    [Fact]
    public void ValidateTelemetry_CanProcessFiveHundredMessagesPerSecondLocally()
    {
        var useCase = new ProcessTelemetryBatch();
        var batches = Enumerable
            .Range(0, 5)
            .Select(batch => Enumerable
                .Range(1, 100)
                .Select(index => new TelemetryMessage(
                    $"DEV-{batch:00}-{index:000}",
                    DateTimeOffset.UtcNow,
                    new GpsDto(4.65, -74.1),
                    48,
                    6.5))
                .ToArray())
            .ToArray();

        var stopwatch = Stopwatch.StartNew();

        foreach (var batch in batches)
        {
            var result = useCase.Validate(batch);
            Assert.True(result.IsSuccess);
        }

        stopwatch.Stop();

        Assert.True(
            stopwatch.Elapsed < TimeSpan.FromSeconds(1),
            $"Se esperaba validar 500 mensajes en menos de 1 segundo, tardo {stopwatch.Elapsed.TotalMilliseconds:N0} ms.");
    }
}
