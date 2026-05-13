namespace CCS.Domain.ValueObjects;

public readonly record struct DeviceId
{
    public DeviceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("El identificador del dispositivo es obligatorio.", nameof(value));
        }

        if (value.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "El identificador del dispositivo no puede superar 100 caracteres.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}

