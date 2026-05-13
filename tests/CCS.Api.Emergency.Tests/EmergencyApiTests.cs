using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CCS.Api.Emergency.Tests;

public sealed class EmergencyApiTests(WebApplicationFactory<Program> factory)
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
        Assert.Equal("CCS.Api.Emergency", body["service"]!.GetValue<string>());
    }

    [Fact]
    public async Task PostEmergency_WhenPanicSignalIsValid_QueuesCriticalActions()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/emergency");
        request.Headers.Add("x-correlation-id", "11111111-1111-1111-1111-111111111111");
        request.Content = Json("""
            {
              "deviceId": "DEV-SMOKE",
              "type": "Panic",
              "source": "Button",
              "gps": { "lat": 4.65, "lng": -74.1 },
              "timestamp": "2026-05-13T12:00:00Z",
              "reason": "panic-button"
            }
            """);

        using var response = await client.SendAsync(request);
        using var debugResponse = await client.GetAsync("/debug/dispatched-actions");
        var actions = JsonNode.Parse(await debugResponse.Content.ReadAsStringAsync())!.AsArray();

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, debugResponse.StatusCode);
        Assert.Contains(actions, action =>
            action?["correlationId"]?.GetValue<string>() == "11111111-1111-1111-1111-111111111111" &&
            action?["deviceId"]?.GetValue<string>() == "DEV-SMOKE" &&
            action?["actions"]?.AsArray().Count == 2);
    }

    [Fact]
    public async Task PostEmergency_WhenEventIsNotCritical_ReturnsBadRequest()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/emergency");
        request.Content = Json("""
            {
              "deviceId": "DEV-SMOKE",
              "type": "OverSpeed",
              "source": "Device",
              "gps": null,
              "timestamp": "2026-05-13T12:00:00Z"
            }
            """);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostEmergency_WhenCorrelationHeaderIsMissing_GeneratesCorrelationId()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/emergency");
        request.Content = Json("""
            {
              "deviceId": "DEV-SMOKE",
              "type": "DriverDistress",
              "source": "MobileApp",
              "gps": null,
              "timestamp": "2026-05-13T12:00:00Z"
            }
            """);

        using var response = await client.SendAsync(request);
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(body["correlationId"]!.GetValue<string>()));
    }

    private static StringContent Json(string body)
    {
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return content;
    }
}
