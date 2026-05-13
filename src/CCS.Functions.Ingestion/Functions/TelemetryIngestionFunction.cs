using CCS.Application.Common;
using CCS.Application.Telemetry;
using CCS.Shared.Contracts;

namespace CCS.Functions.Ingestion.Functions;

public sealed class TelemetryIngestionFunction(ProcessTelemetryBatch processor)
{
    public Result Handle(IReadOnlyCollection<TelemetryMessage> messages)
    {
        return processor.Validate(messages);
    }
}
