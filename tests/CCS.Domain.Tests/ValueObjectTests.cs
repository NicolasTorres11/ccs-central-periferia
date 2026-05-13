using CCS.Domain.ValueObjects;

namespace CCS.Domain.Tests;

public class ValueObjectTests
{
    [Fact]
    public void DeviceId_WhenEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() => new DeviceId(" "));
    }

    [Theory]
    [InlineData(-91, -74)]
    [InlineData(4.7, -181)]
    public void GpsLocation_WhenOutOfRange_Throws(double lat, double lng)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GpsLocation(lat, lng));
    }

    [Fact]
    public void GpsLocation_WhenValid_StoresCoordinates()
    {
        var gps = new GpsLocation(4.711, -74.072);

        Assert.Equal(4.711, gps.Lat);
        Assert.Equal(-74.072, gps.Lng);
    }
}

