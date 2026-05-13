using CCS.Shared.Contracts;

namespace CCS.Application.Emergency;

public sealed record HandlePanicCommand(EmergencySignal Signal, string CorrelationId);

