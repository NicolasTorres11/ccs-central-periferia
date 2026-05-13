namespace CCS.Application.Common;

public sealed record Result(bool IsSuccess, string Status, string? Error = null)
{
    public static Result Success(string status = "ok") => new(true, status);

    public static Result Failure(string error, string status = "failed") => new(false, status, error);
}

