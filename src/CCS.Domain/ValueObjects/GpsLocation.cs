namespace CCS.Domain.ValueObjects;

public readonly record struct GpsLocation
{
    public GpsLocation(double lat, double lng)
    {
        if (lat is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(lat), "La latitud debe estar entre -90 y 90.");
        }

        if (lng is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(lng), "La longitud debe estar entre -180 y 180.");
        }

        Lat = lat;
        Lng = lng;
    }

    public double Lat { get; }

    public double Lng { get; }
}

