using CCS.Application.Common;
using CCS.Domain.ValueObjects;
using CCS.Shared.Contracts;

namespace CCS.Application.Telemetry;

public sealed class ProcessTelemetryBatch
{
    public Result Validate(IReadOnlyCollection<TelemetryMessage> messages)
    {
        if (messages.Count == 0)
        {
            return Result.Failure("El lote de telemetria no puede estar vacio.", "invalid");
        }

        if (messages.Count > 100)
        {
            return Result.Failure("El lote de telemetria no puede superar 100 mensajes.", "invalid");
        }

        foreach (var message in messages)
        {
            _ = new DeviceId(message.DeviceId);

            if (message.Gps is not null)
            {
                _ = new GpsLocation(message.Gps.Lat, message.Gps.Lng);
            }
        }

        return Result.Success("valid");
    }
}

