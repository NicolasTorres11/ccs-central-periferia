using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CCS.Api.Telemetry.Tests;

public sealed class TelemetryApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_ReturnsHealthyServiceName()
    {
        var client = factory.CreateClient();

        using var response = await client.GetAsync("/health");
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("healthy", body["status"]!.GetValue<string>());
        Assert.Equal("CCS.Api.Telemetry", body["service"]!.GetValue<string>());
    }

    [Fact]
    public async Task PostTelemetryIngest_WhenBatchIsValid_StoresMessagesInLocalBuffer()
    {
        var client = factory.CreateClient();
        using var content = Json("""
            [
              {
                "deviceId": "DEV-SMOKE",
                "ts": "2026-05-13T12:00:00Z",
                "gps": { "lat": 4.65, "lng": -74.1 },
                "speedKmh": 48.2,
                "temperatureC": 6.5,
                "eventType": "telemetry"
              }
            ]
            """);

        using var response = await client.PostAsync("/telemetry/ingest", content);
        using var debugResponse = await client.GetAsync("/debug/telemetry");
        var messages = JsonNode.Parse(await debugResponse.Content.ReadAsStringAsync())!.AsArray();

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, debugResponse.StatusCode);
        Assert.Contains(messages, message => message?["deviceId"]?.GetValue<string>() == "DEV-SMOKE");
    }

    [Fact]
    public async Task PostTelemetryIngest_WhenBatchIsEmpty_ReturnsBadRequest()
    {
        var client = factory.CreateClient();

        using var response = await client.PostAsync("/telemetry/ingest", Json("[]"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTelemetryIngest_WhenGpsIsInvalid_ReturnsBadRequest()
    {
        var client = factory.CreateClient();

        using var response = await client.PostAsync("/telemetry/ingest", Json("""
            [
              {
                "deviceId": "DEV-SMOKE",
                "ts": "2026-05-13T12:00:00Z",
                "gps": { "lat": 120, "lng": -74.1 },
                "speedKmh": 48.2,
                "temperatureC": 6.5,
                "eventType": "telemetry"
              }
            ]
            """));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task DebugTelemetry_ReturnsTelemetryCollection()
    {
        var client = factory.CreateClient();

        using var response = await client.GetAsync("/debug/telemetry");
        var messages = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(messages);
    }

    private static StringContent Json(string body)
    {
        return new StringContent(body, Encoding.UTF8, "application/json");
    }
}
